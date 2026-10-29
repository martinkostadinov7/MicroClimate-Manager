#include <Arduino.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>
#include <Wire.h>
#include <Adafruit_AM2320.h>

const char* ssid     = "Odido-91398C";
const char* password = "8EAT4F7P7N7Q7UDH";
const char* serverUrl = "https://microclimatemanager.runasp.net/api";

#define moistureSensor 32

#define pump 25
#define fan 26
#define heater 27

Adafruit_AM2320 am2320 = Adafruit_AM2320();

const unsigned long READ_DATA_INTERVAL = 2000; 
const unsigned long CHECK_RANGES_INTERVAL = 60000; 
const unsigned long CLIMATE_CONTROL_INTERVAL = 10000;   
const unsigned long SEND_DATA_INTERVAL = 60000;   
const unsigned long SOIL_CHECK_INTERVAL = 60 * 60 * 1000UL; // 1 hour (60 minutes)
const unsigned long PUMP_RUN_TIME = 5000;                   // 5 seconds
const unsigned long SOAKING_TIME = 60 * 60 * 1000UL;        // 1 hour
const unsigned long WIFI_RECONNECT_INTERVAL = 30000;        // 30 seconds
const uint16_t HTTP_TIMEOUT = 1500;                         // 1.5 seconds max block

unsigned long previousDataReadTimer = 0;
unsigned long previousCheckRangesTimer = 0;
unsigned long previousClimateCheckTimer = 0;
unsigned long previousSoilCheckTimer = 0;
unsigned long previousWaterPumpTimer = 0;
unsigned long previousDataSendTimer = 0;
unsigned long previousWifiCheckTimer = 0;

int moistureDryValue = 1935; // 0% wet
int moistureWetValue = 1515; // 100% wet

float temperatureMin = 18; 
float temperatureMax = 24; 
float tempOffset = 1.2;
float humidityMin = 60; 
float humidityMax = 80; 

float moistureMin = 30; 
float moistureMax = 70; 

const int arraySize = 30;
float temperatureArray[arraySize];
float humidityArray[arraySize];
float moistureArray[arraySize];

int readingsCount = 0;
int currentIndex = 0;

bool fanRunning = false;
bool heaterRunning = false;
bool initialSettingsLoaded = false;

enum PumpState { IDLE, PUMPING, SOAKING };
PumpState currentPumpState = IDLE;
unsigned long pumpStateTimer = 0;


void readData() {
  unsigned long currentMillis = millis();

  if (currentMillis - previousDataReadTimer >= READ_DATA_INTERVAL) {
    previousDataReadTimer = currentMillis;
    float temperature = am2320.readTemperature();
    if (!isnan(temperature)) {
      temperatureArray[currentIndex] = temperature;
    }
  
    float humidity = am2320.readHumidity();
    if (!isnan(humidity)) {
      humidityArray[currentIndex] = humidity;
    } 
    
    int moistureRawData = analogRead(moistureSensor);
    int moisture = map(moistureRawData, moistureWetValue, moistureDryValue, 100, 0);
    moisture = constrain(moisture, 0, 100);
    moistureArray[currentIndex] = moisture;
    
    currentIndex = (currentIndex + 1) % arraySize;
    if (readingsCount < arraySize) readingsCount++;
  }
}

float getAverage(float array[], int count) {
  if (readingsCount == 0) return 0;
  if (readingsCount < count) {
    count = readingsCount;
  }
  
  float sum = 0;
  int index = (currentIndex - count + arraySize) % arraySize;
  for (int i = 0; i < count; i++) {
    sum += array[index];
    index = (index + 1) % arraySize;
  }

  return sum / count;
}

void sendSensorData() {
  // Не правим мрежови заявки, докато помпата е включена, за да не забавим спирането й
  if (currentPumpState == PUMPING) return;

  unsigned long currentMillis = millis();
  
  if (currentMillis - previousDataSendTimer >= SEND_DATA_INTERVAL) {
    previousDataSendTimer = currentMillis;
    float temperatureAverage = getAverage(temperatureArray, arraySize) - tempOffset;
    float humidityAverage = getAverage(humidityArray, arraySize);
    float moistureAverage = getAverage(moistureArray, arraySize);
    
    if (WiFi.status() == WL_CONNECTED) {
      HTTPClient http;
      String sensorReadingsUrl = serverUrl + String("/sensorReadings");
      http.begin(sensorReadingsUrl);
      http.setConnectTimeout(HTTP_TIMEOUT);
      http.setTimeout(HTTP_TIMEOUT);
      http.addHeader("Content-Type", "application/json");
      
      JsonDocument doc;
      
      doc["temperature"] = round(temperatureAverage * 10.0) / 10.0;
      doc["humidity"] = round(humidityAverage * 10.0) / 10.0;
      doc["soilMoisture"] = round(moistureAverage * 10.0) / 10.0;
      
      String jsonPayload;
      serializeJson(doc, jsonPayload);
      
      Serial.println("HTTP: Sending sensor data!");
      http.POST(jsonPayload);
      http.end();
    } else {
      Serial.println("No wifi connection! Skipping sensor data send.");
    }
  }
}

void sendActuatorData(String deviceName, int state) {
  if (WiFi.status() == WL_CONNECTED) {
    HTTPClient http;
    String sensorReadingsUrl = serverUrl + String("/actuatorLogs");
    http.begin(sensorReadingsUrl);
    http.setConnectTimeout(HTTP_TIMEOUT);
    http.setTimeout(HTTP_TIMEOUT);
    http.addHeader("Content-Type", "application/json");
    
    JsonDocument doc;

    doc["deviceName"] = deviceName;
    doc["state"] = state;
    
    String jsonPayload;
    serializeJson(doc, jsonPayload);
    
    Serial.println("HTTP: Sending actuator info");
    http.POST(jsonPayload);
    http.end();
  } else {
    Serial.println("No wifi connection! Actuator state changed locally.");
  }
}

void handleClimateControl() {
  unsigned long currentMillis = millis();
  
  if (currentMillis - previousClimateCheckTimer >= CLIMATE_CONTROL_INTERVAL) {
    previousClimateCheckTimer = currentMillis;
    if (readingsCount == 0)
      return;
      
    float temperatureAverage = getAverage(temperatureArray, 5);
    float humidityAverage = getAverage(humidityArray, 5);
  
    float temperatureMiddle = (temperatureMax + temperatureMin) / 2;
    float humidityMiddle = (humidityMax + humidityMin) / 2;
    
    if (heaterRunning && (temperatureAverage > temperatureMiddle)) {
      digitalWrite(heater, LOW);
      heaterRunning = false;
      sendActuatorData("heater", 0);
    }
  
    if (fanRunning && (humidityAverage < humidityMiddle)) {
      digitalWrite(fan, LOW);
      fanRunning = false;
      sendActuatorData("fan", 0);
    }
    
    if (!fanRunning && (humidityAverage > humidityMax)) {
      digitalWrite(fan, HIGH);
      fanRunning = true;
      sendActuatorData("fan", 1);
    }
  
    if (!heaterRunning && (temperatureAverage < temperatureMin)) {
      if (currentPumpState != PumpState::PUMPING) {
        digitalWrite(heater, HIGH);
        heaterRunning = true;
        sendActuatorData("heater", 1);
      }
    }
  }
}

void handleWatering() {
  unsigned long currentMillis = millis();
  
  switch (currentPumpState) {
    case IDLE:
      if (currentMillis - previousSoilCheckTimer >= SOIL_CHECK_INTERVAL) {
        previousSoilCheckTimer = currentMillis;
        float moistureAvg = getAverage(moistureArray, 5);
        if (readingsCount > 0 && moistureAvg < moistureMin) {
          bool heaterWasTurnedOff = false;
          if (heaterRunning) {
            digitalWrite(heater, LOW);
            heaterRunning = false;
            heaterWasTurnedOff = true;
          }
          
          // Първо включваме помпата и засичаме точното време, преди HTTP заявките
          digitalWrite(pump, HIGH);
          pumpStateTimer = millis();
          currentPumpState = PUMPING;

          if (heaterWasTurnedOff) {
            sendActuatorData("heater", 0);
          }
          sendActuatorData("pump", 1);
        }
      }
      break;
      
    case PUMPING:
      if (currentMillis - pumpStateTimer >= PUMP_RUN_TIME) {
        digitalWrite(pump, LOW);
        pumpStateTimer = millis();
        currentPumpState = SOAKING;
        sendActuatorData("pump", 0);
      }
      break;
      
    case SOAKING:
      if (currentMillis - pumpStateTimer >= SOAKING_TIME) {
        currentPumpState = IDLE;
      }
      break;
  }
}

void applyRanges() {
  if (currentPumpState == PUMPING) return;

  unsigned long currentMillis = millis();
  if (currentMillis - previousCheckRangesTimer < CHECK_RANGES_INTERVAL) {
    return;
  }
  previousCheckRangesTimer = currentMillis;
  
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("No wifi connection! Using current range settings.");
    return;
  }

  HTTPClient http;
  String settingsUrl = serverUrl + String("/settings/unapplied");
  http.begin(settingsUrl);
  http.setConnectTimeout(HTTP_TIMEOUT);
  http.setTimeout(HTTP_TIMEOUT);
  http.addHeader("Content-Type", "application/json");
  
  Serial.println("HTTP: Getting unapplied settings");
  int httpResponseCode = http.GET();
  
  if (httpResponseCode == HTTP_CODE_OK) {
    String payload = http.getString();
    http.end();
    
    JsonDocument doc;
    DeserializationError error = deserializeJson(doc, payload);
    
    if (error) {
      Serial.print("JSON Parsing failed: ");
      Serial.println(error.c_str());
      return;
    }

    JsonArray settings = doc.as<JsonArray>();
    
    if (settings.size() > 0) {
      for (JsonObject setting : settings) {
        String settingName = setting["settingName"];
        float parsedValue = setting["value"].as<String>().toFloat();

        if (settingName == "TemperatureMin") temperatureMin = parsedValue;
        else if (settingName == "TemperatureMax") temperatureMax = parsedValue;
        else if (settingName == "HumidityMin") humidityMin = parsedValue;
        else if (settingName == "HumidityMax") humidityMax = parsedValue;
        else if (settingName == "MoistureMin") moistureMin = parsedValue;
        else if (settingName == "MoistureMax") moistureMax = parsedValue;
      }

      String applySettingsUrl = serverUrl + String("/settings/apply");
      http.begin(applySettingsUrl);
      http.setConnectTimeout(HTTP_TIMEOUT);
      http.setTimeout(HTTP_TIMEOUT);
      http.addHeader("Content-Type", "application/json");
      
      String jsonPayload;
      serializeJson(doc, jsonPayload);
      
      Serial.println("HTTP: Sending settings to apply");
      http.POST(jsonPayload);
      http.end();
    } else {
      Serial.println("No unapplied settings found.");
    }

  } else {
    Serial.print("HTTP GET Error code: ");
    Serial.println(httpResponseCode);
    http.end();
  }
}

void retreiveLatestSettings() {
  if (WiFi.status() != WL_CONNECTED) return;

  HTTPClient http;
  String settingsUrl = serverUrl + String("/settings");
  http.begin(settingsUrl);
  http.setConnectTimeout(HTTP_TIMEOUT);
  http.setTimeout(HTTP_TIMEOUT);
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.GET();

  if (httpResponseCode == HTTP_CODE_OK) {
    String payload = http.getString();
    http.end();

    JsonDocument doc;
    DeserializationError error = deserializeJson(doc, payload);

    if (error) {
      Serial.print("JSON Parsing failed: ");
      Serial.println(error.c_str());
      return;
    }

    JsonArray settings = doc.as<JsonArray>();

    if (settings.size() > 0) {
      for (JsonObject setting : settings) {
        String settingName = setting["settingName"];
        float parsedValue = setting["value"].as<String>().toFloat();

        if (settingName == "TemperatureMin") temperatureMin = parsedValue;
        else if (settingName == "TemperatureMax") temperatureMax = parsedValue;
        else if (settingName == "HumidityMin") humidityMin = parsedValue;
        else if (settingName == "HumidityMax") humidityMax = parsedValue;
        else if (settingName == "MoistureMin") moistureMin = parsedValue;
        else if (settingName == "MoistureMax") moistureMax = parsedValue;
        Serial.println(settingName + " - " + String(parsedValue));
      }
    }
    initialSettingsLoaded = true;
  } else {
    Serial.print("HTTP GET Error code: ");
    Serial.println(httpResponseCode);
    http.end();
  }
}

void handleWiFi() {
  if (currentPumpState == PUMPING) return;

  // Ако сме стартирали офлайн и връзката се е появила по-късно, взимаме настройките еднократно
  if (WiFi.status() == WL_CONNECTED && !initialSettingsLoaded) {
    Serial.println("WiFi connected! Syncing initial state...");
    sendActuatorData("heater", heaterRunning ? 1 : 0);
    sendActuatorData("fan", fanRunning ? 1 : 0);
    sendActuatorData("pump", 0);
    retreiveLatestSettings();
  }

  // Неблокиращ опит за пресвързване на всеки 30 секунди
  unsigned long currentMillis = millis();
  if (currentMillis - previousWifiCheckTimer >= WIFI_RECONNECT_INTERVAL) {
    previousWifiCheckTimer = currentMillis;
    if (WiFi.status() != WL_CONNECTED) {
      Serial.println("WiFi lost. Reconnecting in background...");
      WiFi.disconnect();
      WiFi.reconnect();
    }
  }
}

void setup() {
  Serial.begin(115200);
  pinMode(moistureSensor, INPUT);

  pinMode(fan, OUTPUT);
  pinMode(pump, OUTPUT);
  pinMode(heater, OUTPUT);

  digitalWrite(fan, LOW);
  digitalWrite(pump, LOW);
  digitalWrite(heater, LOW);

  am2320.begin();

  WiFi.mode(WIFI_STA);
  WiFi.setAutoReconnect(true);
  WiFi.begin(ssid, password);
  
  Serial.print("Connecting to wifi");
  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED && attempts < 10) {
    delay(500);
    Serial.print(".");
    attempts++;
  }
  Serial.println();

  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("Connected!");
    sendActuatorData("heater", 0);
    sendActuatorData("fan", 0);
    sendActuatorData("pump", 0);
    retreiveLatestSettings();
  } else {
    Serial.println("Starting in offline mode with default settings.");
  }
}

void loop() {
  readData();
  handleClimateControl();
  handleWatering();
  handleWiFi();
  applyRanges();
  sendSensorData();
}
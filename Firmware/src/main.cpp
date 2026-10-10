#include <Arduino.h>
#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>

  const char* ssid     = "H770";
  const char* password = "123456789";
  const char* serverUrl = "http://192.168.137.1:5029/api";

#define DHTPIN 33
#define DHTTYPE DHT11

#define moistureSensor 32

#define pump 25
#define fan 26
#define heater 27

DHT dht(DHTPIN, DHTTYPE);

const unsigned long READ_DATA_INTERVAL = 2000; 
const unsigned long CHECK_RANGES_INTERVAL = 60000; 
const unsigned long CLIMATE_CONTROL_INTERVAL = 10000;   
const unsigned long SEND_DATA_INTERVAL = 60000;   
const unsigned long SOIL_CHECK_INTERVAL = 10000;
const unsigned long PUMP_RUN_TIME = 5000;  // watering for 5 seconds
const unsigned long SOAKING_TIME = 60000; // waiting for water soak for 1 minute

unsigned long previousDataReadTimer = 0;
unsigned long previousCheckRangesTimer = 0;
unsigned long previousClimateCheckTimer = 0;
unsigned long previousSoilCheckTimer = 0;
unsigned long previousWaterPumpTimer = 0;
unsigned long previousDataSendTimer = 0;

int moistureAirValue = 1980;   // 0% wet
int moistureWaterValue = 4095; // 100% wet

float temperatureMax = 30; 
float temperatureMin = 26; 

float humidityMax = 90; 
float humidityMin = 60; 

float moistureMax = 70; 
float moistureMin = 30; 

const int arraySize = 30;
float temperatureArray[arraySize];
float humidityArray[arraySize];
float moistureArray[arraySize];

int readingsCount = 0;
int currentIndex = 0;

bool fanRunning = false;
bool heaterRunning = false;

enum PumpState { IDLE, PUMPING, SOAKING };
PumpState currentPumpState = IDLE;
unsigned long pumpStateTimer = 0;

void setup() {
  Serial.begin(115200);
  pinMode(moistureSensor, INPUT);

  pinMode(fan, OUTPUT);
  pinMode(pump, OUTPUT);
  pinMode(heater, OUTPUT);

  WiFi.begin(ssid, password);
  Serial.println("Connecting to wifi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("Connected!");

  dht.begin();

   HTTPClient http;
  String settingsUrl = serverUrl + String("/settings");
  http.begin(settingsUrl);
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

    } else {
      Serial.print("HTTP GET Error code: ");
      Serial.println(httpResponseCode);
      http.end();
    }
  }
}

void readData(){
  unsigned long currentMillis = millis();

  if (currentMillis - previousDataReadTimer >= READ_DATA_INTERVAL) {
    previousDataReadTimer = currentMillis;
    float temperature = dht.readTemperature();
    if (!isnan(temperature)) {
      temperatureArray[currentIndex] = temperature;
    }
  
    float humidity = dht.readHumidity();
    if (!isnan(humidity)) {
      humidityArray[currentIndex] = humidity;
    } 
    
    int moistureRawData = analogRead(moistureSensor);
    int moisture = map(moistureRawData, moistureAirValue, moistureWaterValue, 0, 100);
    moisture = constrain(moisture, 0, 100);
    moistureArray[currentIndex] = moisture;
  
    currentIndex = (currentIndex + 1) % arraySize;
    if(readingsCount < arraySize) readingsCount++;
  }
}

float getAverage(float array[], int count){
  if (readingsCount == 0) return 0;
  if (readingsCount < count)
  {
    count = readingsCount;
  }
  
  float average = 0;
  float sum = 0;
  int index = (currentIndex - count + arraySize) % arraySize;
  for (int i = 0; i < count; i++)
  {
    sum += array[index];
    index = (index + 1) % arraySize;
  }

  average = sum / count;
  return average;
}

void sendSensorData(){
  unsigned long currentMillis = millis();

  if (currentMillis - previousDataSendTimer >= SEND_DATA_INTERVAL) {
    previousDataSendTimer = currentMillis;
    float temperatureAverage = getAverage(temperatureArray, arraySize);
    float humidityAverage = getAverage(humidityArray, arraySize);
    float moistureAverage = getAverage(moistureArray, arraySize);

    if (WiFi.status() == WL_CONNECTED) {
      HTTPClient http;
      String sensorReadingsUrl = serverUrl + String("/sensorReadings");
      http.begin(sensorReadingsUrl);
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
      Serial.println("No wifi connection!");
    }
  }
}

void sendActuatorData(String deviceName, int state){
  if (WiFi.status() == WL_CONNECTED) {
      HTTPClient http;
      String sensorReadingsUrl = serverUrl + String("/actuatorLogs");
      http.begin(sensorReadingsUrl);
      http.addHeader("Content-Type", "application/json");
      
      JsonDocument doc;

      doc["deviceName"] = deviceName;
      doc["state"] = state;

      String jsonPayload;
      serializeJson(doc, jsonPayload);

      Serial.println("HTTP: Sending actuator info");
      int httpResponseCode = http.POST(jsonPayload);
      http.end();
    } else {
      Serial.println("No wifi connection!");
    }
}

void handleClimateControl(){
  unsigned long currentMillis = millis();

  if (currentMillis - previousClimateCheckTimer >= CLIMATE_CONTROL_INTERVAL) {
    previousClimateCheckTimer = currentMillis;
    if (readingsCount == 0)
      return;
    
    float temperatureAverage = getAverage(temperatureArray, 5);
    float humidityAverage = getAverage(humidityArray, 5);
  
    float temperatureMiddle = (temperatureMax + temperatureMin) / 2;
    float humidityMiddle = (humidityMax + humidityMin) / 2;
  
    if (heaterRunning && (temperatureAverage > temperatureMiddle)) // temperature is in optimal range
    {
      digitalWrite(heater, LOW);
      sendActuatorData("heater", 0);
      heaterRunning = false;
    }
  
    if (fanRunning && (humidityAverage < humidityMiddle)) // humidity is in optimal range
    {
      digitalWrite(fan, LOW);
      sendActuatorData("fan", 0);
      fanRunning = false;
    }
    
  
    if (!fanRunning && (humidityAverage > humidityMax)) // high humidity
    {
      digitalWrite(fan, HIGH);
      sendActuatorData("fan", 1);
      fanRunning = true;
    }
  
    if (!heaterRunning && (temperatureAverage < temperatureMin)) // low temperature
    {
      if (currentPumpState != PumpState::PUMPING)
      {
        digitalWrite(heater, HIGH);
        sendActuatorData("heater", 1);
        heaterRunning = true;
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
            if (heaterRunning){
              digitalWrite(heater, LOW);
              sendActuatorData("heater", 0);
              heaterRunning = false;
            }
            digitalWrite(pump, HIGH);
            sendActuatorData("pump", 1);
            pumpStateTimer = currentMillis;
            currentPumpState = PUMPING;
          }
    }
      break;

    case PUMPING:
      if (currentMillis - pumpStateTimer >= PUMP_RUN_TIME) {
        digitalWrite(pump, LOW);
        sendActuatorData("pump", 0);
        pumpStateTimer = currentMillis;
        currentPumpState = SOAKING;
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
  unsigned long currentMillis = millis();
  if (currentMillis - previousCheckRangesTimer < CHECK_RANGES_INTERVAL) {
    return;
  }
  previousCheckRangesTimer = currentMillis;

  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("No wifi connection!");
    return;
  }

  HTTPClient http;
  String settingsUrl = serverUrl + String("/settings/unapplied");
  http.begin(settingsUrl);
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

void loop() {
  readData();
  applyRanges();
  handleClimateControl();
  handleWatering();
  sendSensorData();
}
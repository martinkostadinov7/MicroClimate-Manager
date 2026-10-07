#include <Arduino.h>
#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <WiFi.h>
#include <HTTPClient.h>

const char* ssid     = "H770";
const char* password = "123456789";
const char* serverUrl = "http://192.168.137.1:5000/";

#define DHTPIN 33
#define DHTTYPE DHT11

#define moistureSensor 32

#define pump 25
#define fan 26
#define heater 27

DHT dht(DHTPIN, DHTTYPE);

const unsigned long READ_DATA_INTERVAL = 2000; 
const unsigned long CLIMATE_CONTROL_INTERVAL = 10000;   
const unsigned long SEND_DATA_INTERVAL = 60000;   
const unsigned long SOIL_CHECK_INTERVAL = 10000;
const unsigned long PUMP_RUN_TIME = 5000;  // watering for 5 seconds
const unsigned long SOAKING_TIME = 60000; // waiting for water soak for 1 minute


unsigned long previousSoilCheckTimer = 0;
unsigned long previousDataReadTimer = 0;
unsigned long previousDataSendTimer = 0;
unsigned long previousClimateCheckTimer = 0;
unsigned long previousWaterPumpTimer = 0;

int moistureAirValue = 2000;   // 0% wet
int moistureWaterValue = 1600; // 100% wet

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

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  dht.begin();
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

void sendData(){
  unsigned long currentMillis = millis();

  if (WiFi.status() == WL_CONNECTED) {
    HTTPClient http;

    http.begin(serverUrl);
    http.addHeader("Content-Type", "application/json");
    String jsonPayload = "{\"status\":\"test\", \"message\":\"Hello from ESP32!\"}";

    Serial.println("Sending request to server");
    
    int httpResponseCode = http.POST(jsonPayload);

    if (httpResponseCode > 0) {
      Serial.print("Success code: ");
      Serial.println(httpResponseCode);
    } else {
      Serial.print("Error: ");
      Serial.println(httpResponseCode);
    }

    http.end();
  } else {
    Serial.println("No wifi connection!");
  }
  if (currentMillis - previousDataSendTimer >= SEND_DATA_INTERVAL) {
    
    Serial.println("Sending data!");
    previousDataSendTimer = currentMillis;
    float temperatureAverage = getAverage(temperatureArray, arraySize);
    float humidityAverage = getAverage(humidityArray, arraySize);
    float moistureAverage = getAverage(moistureArray, arraySize);

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
  
    Serial.print("Checking climate metrics - Temp: ");
    Serial.print(temperatureAverage, 1); // Показва 1 знак след запетаята
    Serial.print(" °C | Humidity: ");
    Serial.print(humidityAverage, 1);
    Serial.println(" %");
  
    if (heaterRunning && (temperatureAverage > temperatureMiddle)) // temperature is in optimal range
    {
      digitalWrite(heater, LOW);
      Serial.println("Temperature is ok, heater off");
      heaterRunning = false;
    }
  
    if (fanRunning && (humidityAverage < humidityMiddle)) // humidity is in optimal range
    {
      digitalWrite(fan, LOW);
      Serial.println("Humidity is ok, fan off");
      fanRunning = false;
    }
    
  
    if (!fanRunning && (humidityAverage > humidityMax)) // high humidity
    {
      digitalWrite(fan, HIGH);
      Serial.println("Humidity is high, activating fan");
      fanRunning = true;
    }
  
    if (!heaterRunning && (temperatureAverage < temperatureMin)) // low temperature
    {
      if (currentPumpState != PumpState::PUMPING)
      {
        digitalWrite(heater, HIGH);
        Serial.println("Temperature is low, activating heater");
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
        Serial.print(" °C | Soil Moisture: ");
        Serial.print(moistureAvg, 1);
        Serial.println(" %");
          if (readingsCount > 0 && moistureAvg < moistureMin) {
            if (heaterRunning)
              digitalWrite(heater, LOW);
              heaterRunning = false;
            Serial.println("Soil is dry, starting water pump");
            digitalWrite(pump, HIGH);
            pumpStateTimer = currentMillis;
            currentPumpState = PUMPING;
          }
    }
      break;

    case PUMPING:
      if (currentMillis - pumpStateTimer >= PUMP_RUN_TIME) {
        digitalWrite(pump, LOW);
        Serial.println("Pumping over, starting to soak.");
        pumpStateTimer = currentMillis;
        currentPumpState = SOAKING;
      }
      break;

    case SOAKING:
      if (currentMillis - pumpStateTimer >= SOAKING_TIME) {
        currentPumpState = IDLE;
        Serial.println("Soaking is done, checking soil moisture!");
      }
      break;
  }
}


void loop() {
  readData();
  handleClimateControl();
  handleWatering();
  sendData();
}
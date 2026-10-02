#include <Arduino.h>

#include <WiFi.h>
#include <HTTPClient.h>

// 1. Въведете точното име и парола на Hotspot-а от лаптопа
const char* ssid     = "H770";
const char* password = "123456789";

// 2. Адресът на вашия лаптоп (порт 5000)
const char* serverUrl = "http://192.168.137.1:5000/";

void setup() {
  Serial.begin(115200);
  delay(1000);

  // Свързване към Wi-Fi
  Serial.print("Connecting to ");
  Serial.println(ssid);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("Wfi is connected!");
  Serial.print("IP address of ESP32: ");
  Serial.println(WiFi.localIP());
}

void loop() {
  // Проверяваме дали все още имаме Wi-Fi връзка
  if (WiFi.status() == WL_CONNECTED) {
    HTTPClient http;

    // Инициализираме HTTP връзката
    http.begin(serverUrl);
    http.addHeader("Content-Type", "application/json");

    // Тестов JSON текст
    String jsonPayload = "{\"status\":\"test\", \"message\":\"Hello from ESP32!\"}";

    Serial.println("Sending request to laptop...");
    
    // Изпращаме HTTP POST заявка
    int httpResponseCode = http.POST(jsonPayload);

    // Ако кодът е положителен (напр. 200), заявката е стигнала успешно
    if (httpResponseCode > 0) {
      Serial.print("Success code form laptop: ");
      Serial.println(httpResponseCode);
    } else {
      Serial.print("error: ");
      Serial.println(httpResponseCode);
    }

    // Затваряме връзката
    http.end();
  } else {
    Serial.println("No Wi-Fi connection!");
  }

  // Изчакваме 5 секунди преди следващото изпращане
  delay(5000);
}
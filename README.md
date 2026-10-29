# MicroClimate Manager

**MicroClimate Manager** is an automated IoT solution designed for greenhouse owners to seamlessly regulate soil moisture and climate control. By utilizing smart irrigation, heating, and ventilation, this system tackles the common challenges of maintaining optimal environmental levels, providing all real-time data through an interactive web application.

## Features

* **Real-Time Environmental Monitoring**: Tracks temperature, humidity, and soil moisture using hardware sensors.
* **Smart Automation**: 
  * **Irrigation**: Implements a 3-state system (Idle, Pumping, Soaking) to prevent overwatering.
  * **Climate Control**: Controls a heating element and ventilation fan utilizing hysteresis to prevent actuator flickering.
* **Data Smoothing**: Uses a circular buffer to calculate the average of recent sensor readings, ensuring stable and reliable automation triggers.
* **Interactive Web Dashboard**: 
  * View live environment data and actuator states (updated every 60 seconds or via manual refresh).
  * Visualize historical data (Temperature, Humidity, and Moisture) with customizable time ranges using **Chart.js**.
  * Review 24-hour statistics (Min, Avg, Max).
* **Remote Configuration**: Adjust minimum and maximum thresholds for temperature, humidity, and soil moisture directly from the web app, which syncs with the microcontroller every minute.

## Tech Stack & Tools

### Hardware (Firmware)
* **Microcontroller**: ESP32 WiFi
* **Environment Sensor**: AM2320 (Temperature & Humidity)
* **Soil Sensor/Pump**: M5Stack Watering Unit
* **Actuators**: Cement Resistor (Heater, 10W 5Ω), Axial 5V Fan
* **Other Components**: N-channel MOSFET IRLB8721, 5V 2A Adapter, Breadboard
* **Language/Environment**: C++ (PlatformIO)
* **Libraries**: `Adafruit_AM2320.h`, `WiFi.h`, `HTTPClient.h`, `ArduinoJson.h`

### Backend (API & Database)
* **Framework**: ASP.NET Core Web API (C#)
* **Architecture**: N-Tier (Presentation, Business, Repository layers) with DTOs for data transfer.
* **Database**: SQL Server configured via Entity Framework Core (Code-First approach).
* **Documentation & Testing**: Swagger, Postman

### Frontend (Web Application)
* **Technologies**: HTML5, CSS3, Vanilla JavaScript
* **Libraries**: Chart.js (for data visualization)

## System Architecture

1. **Edge (ESP32)**: Captures sensor data every 2 seconds into a circular buffer. Every 10 seconds, it evaluates the average of the last 5 readings to trigger the fan, heater, or water pump. It POSTs data to the backend and GETs updated settings every 1 minute.
2. **API (ASP.NET)**: Receives HTTP requests from the ESP32 and the Web App. Processes business logic and stores/retrieves data from the SQL Server database.
3. **Client (Web App)**: Fetches the latest sensor readings, 24-hour statistics, and historical chart data from the API. Allows users to POST new threshold settings back to the database.

## Live Demo
The web application is currently deployed and accessible at:
[MicroClimate Manager Dashboard]([https://i594226.luna.fhict.nl/dashboard/index.html](https://i594226.hera.fontysict.net/index.html)) *(Note: Availability depends on active hosting)*

## Repository Structure

* `/Firmware`: C++ PlatformIO project containing the ESP32 code.
* `/Backend`: ASP.NET Core API project, including EF Core database migrations and N-Tier architecture folders.
* `/Frontend`: HTML, CSS, and JS files for the web dashboard.
* `Realisation.docx`: Detailed project realization report.

## Author
**Martin Kostadinov**

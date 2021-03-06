#include <WiFi.h>   
#include "AzureIotHub.h"
#include "Esp32MQTTClient.h" 
#include <DHT.h>
#include <ArduinoJson.h>
#define DHT_PIN 33
#define DHT_TYPE DHT11
//AZURE SETTINGS
char *ssid = " SSID";
char *pass = " PASSWORD ";
char *conn = " YOUR CONNECTION STRING";
bool messagePending = false;
int interval = 1000;
unsigned long prevMillis = 0;
//DHT SETTINGS
int temperatureAlert = 0;


time_t epochTime;
DHT dht(DHT_PIN,DHT_TYPE);
IOTHUB_CLIENT_LL_HANDLE deviceClient;

void setup() {
  initSerial();
  initWifi();
  initEpochTime();
  initDHT();
  initDevice();
  delay(2000);
}

void loop() {
  unsigned long currentMillis = millis();
  epochTime = time(NULL);
  float temperature = dht.readTemperature();
  float humidity = dht.readHumidity();
  if(!messagePending) {
    if((currentMillis - prevMillis) >= interval) {
      prevMillis = currentMillis; 
     if(!(std::isnan(temperature)) && !(std::isnan(humidity)) && epochTime > 28800){ 
         if(temperature >= 30 || temperature < 10){ 
            temperatureAlert = 1;    
          }
          if(temperature < 30 && temperature > 10){ 
             temperatureAlert = 0; 
            }
                Serial.printf("Current Time: %lu. ", epochTime);            
                char payload[256];
                char epochTimeBuf[12];   
                StaticJsonBuffer<sizeof(payload)> buf;
                JsonObject &root = buf.createObject();
                root["temperature"] = temperature;
                root["humidity"] = humidity;  
                root["temperatureAlert"] = temperatureAlert;
                root["epochTime"] = epochTime;
                root.printTo(payload, sizeof(payload));
                sendMessage(payload, itoa(epochTime, epochTimeBuf, 10)); 
             }
         }     
     }
 IoTHubClient_LL_DoWork(deviceClient);
 delay(2000);    
 } 
        

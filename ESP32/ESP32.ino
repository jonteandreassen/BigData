#include <WiFi.h>   
#include "AzureIotHub.h"
#include "Esp32MQTTClient.h" 
#include <DHT.h>
#include <ArduinoJson.h>
#define DHT_PIN 33
#define DHT_TYPE DHT11
//AZURE SETTINGS
char *ssid = "JontesWiFi";
char *pass = "%casaDeJonte";
char *conn = "HostName=jonteIotHub.azure-devices.net;DeviceId=B8:F0:09:CC:ED:EC;SharedAccessKey=UnUsHpdrEwilYk9gW6QpueRSU0wOLWz5qE0Hs0Vjy1w=";
bool messagePending = false;
int interval = 1000;
unsigned long prevMillis = 0;
//DHT SETTINGS
//  float diff = 1.0;
//  float prevTemp = 0.0;
  //int temperatureAlert = 0;


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
                Serial.printf("Current Time: %lu. ", epochTime);            
                char payload[256];
                char epochTimeBuf[12];   
                StaticJsonBuffer<sizeof(payload)> buf;
                JsonObject &root = buf.createObject();
                root["temperature"] = temperature;
                root["humidity"] = humidity;  
                root["temperatureAlert"] = "1";
                root["epochTime"] = epochTime;
                root.printTo(payload, sizeof(payload));
                sendMessage(payload, itoa(epochTime, epochTimeBuf, 10)); 
             }
         }     
     }
 IoTHubClient_LL_DoWork(deviceClient);
 delay(2000);    
 } 
        

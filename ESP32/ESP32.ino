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
int interval = 10 * 1000;
unsigned long prevMillis = 0;
//DHT SETTINGS
float diff = 1.0;
float prevTemp = 0.0;
int temperatureAlert = 0;


time_t epochTime;
DHT dht(DHT_PIN,DHT_TYPE);
IOTHUB_CLIENT_LL_HANDLE deviceClient;

void initSerial() {
  Serial.begin(115200);
  delay(2000);
  Serial.println("Serial initialized.");
} 

void initWifi() {
  WiFi.begin(ssid, pass);

  while(WiFi.status() != WL_CONNECTED) {
    delay(1000);
  }

  Serial.println("WiFi initialized.");
}

void initEpochTime() {
  configTime(3600, 0, "pool.ntp.org", "time.nist.gov");

  while(true) {
    epochTime = time(NULL);

    if(epochTime == 28800) {
      delay(2000);
    } else {
      break;
    }
  }

  Serial.printf("Epochtime initialized. Current Time: %lu \n", epochTime);
}

void initDHT() {
  dht.begin();
  Serial.println("DHT initialized.");
}




void initDevice() {
  deviceClient = IoTHubClient_LL_CreateFromConnectionString(conn, MQTT_Protocol);
}

void sendCallback(IOTHUB_CLIENT_CONFIRMATION_RESULT result, void *userContextCallback) {
  if(IOTHUB_CLIENT_CONFIRMATION_OK == result) {
    Serial.println("Sending message to Azure IoT Hub - succeeded.");
  }
  
  messagePending = false;
}

void sendMessage(char *payload, char *epochTime) {
  
  IOTHUB_MESSAGE_HANDLE message = IoTHubMessage_CreateFromByteArray((const unsigned char *) payload, strlen(payload));
  MAP_HANDLE properties = IoTHubMessage_Properties(message);
    Map_Add(properties, "type", "DHT");
    Map_Add(properties, "latitude", "59.2982911");
    Map_Add(properties, "longitude", "18.0989677");
    Map_Add(properties, "vendor", "AzDelivery");
    Map_Add(properties, "model", "Dev Module");
    Map_Add(properties, "typeName", "ESP32");
    Map_Add(properties, "deviceName", "ESP32wroom32");
    Map_Add(properties, "macAdress", "B8:F0:09:CC:ED:EC");
  if(IoTHubClient_LL_SendEventAsync(deviceClient, message, sendCallback, NULL) == IOTHUB_CLIENT_OK) {
    messagePending = true;
  }

  IoTHubMessage_Destroy(message);
}


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
        

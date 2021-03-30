

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

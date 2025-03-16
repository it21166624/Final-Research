#include <ESP8266WiFi.h>
#include <PubSubClient.h>

#define relay_pin 4
// WiFi Credentials
const char* ssid = "Chana’s iPhone";
const char* password = "123456789";

// MQTT Broker Info (Change if running remotely)
const char* mqtt_server = "172.20.10.3";  // Replace with your machine's IP address
const int mqtt_port = 1883;
const char* mqtt_topic = "poultry/light_status";

WiFiClient espClient;
PubSubClient client(espClient);

const int LDR_PIN = A0;     // LDR Sensor connected to A0
const int THRESHOLD = 700;  // Adjust this based on your lighting conditions


void setup_wifi() {
  Serial.print("Connecting to WiFi...");
  WiFi.begin(ssid, password);
  pinMode(relay_pin, OUTPUT);

  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.print(".");
  }
  Serial.println("\n✅ WiFi Connected!");
  Serial.print("IP Address: ");
  Serial.println(WiFi.localIP());
}

void reconnect() {
  while (!client.connected()) {
    Serial.print("Connecting to MQTT...");
    if (client.connect("ESP8266Client")) {
      Serial.println("✅ Connected to MQTT Broker!");
    } else {
      Serial.print("❌ Failed, retrying in 5 seconds...");
      delay(5000);
    }
  }
}

void setup() {
  Serial.begin(9600);
  setup_wifi();
  client.setServer(mqtt_server, mqtt_port);
}

void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();

  int ldrValue = analogRead(LDR_PIN);
  String lightStatus = "OFF";

  if (ldrValue > THRESHOLD) {
    lightStatus = "ON";
    digitalWrite(relay_pin, LOW);
  } else {
    lightStatus = "OFF";
    digitalWrite(relay_pin, HIGH);
  }


  Serial.print("LDR Value: ");
  Serial.print(ldrValue);
  Serial.print(" | Light Status: ");
  Serial.println(lightStatus);

  // Create JSON payload
  String payload = "{";
  payload += "\"LDRValue\": " + String(ldrValue) + ",";
  payload += "\"Status\": \"" + lightStatus + "\"";
  payload += "}";

  // Publish data to MQTT
  client.publish(mqtt_topic, payload.c_str());

  delay(5000);  // Publish every 5 seconds
}

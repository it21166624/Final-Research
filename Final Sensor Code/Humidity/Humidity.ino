#include "DHT.h"
#include <ESP8266WiFi.h>
#include <PubSubClient.h>

#define DHTPIN D4
#define DHTTYPE DHT11

DHT dht(DHTPIN, DHTTYPE);
#define relay_pin 4
// WiFi Credentials
const char* ssid = "iPhone";
const char* password = "24681012";

// MQTT Broker Info (Change if running remotely)
const char* mqtt_server = "172.20.10.5";  // Replace with your machine's IP address
const int mqtt_port = 1883;
const char* mqtt_topic = "sensor/data";

// Create WiFi and MQTT Client objects
WiFiClient espClient;
PubSubClient client(espClient);

void setup() {
  Serial.begin(9600);
  pinMode(relay_pin, OUTPUT);
  dht.begin();
  Serial.println("DHT11 Sensor Test");

  // Connect to WiFi
  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi...");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nConnected to WiFi!");

  // Connect to MQTT Broker
  client.setServer(mqtt_server, mqtt_port);
  while (!client.connected()) {
    Serial.print("Connecting to MQTT Broker...");
    if (client.connect("NodeMCU_Client")) {
      Serial.println("Connected!");
    } else {
      Serial.print("Failed, rc=");
      Serial.print(client.state());
      Serial.println(" Retrying in 5 seconds...");
      delay(3000);
    }
  }
}

void loop() {
  delay(2000);

float humidity = dht.readHumidity();

  if (isnan(humidity)) {
    Serial.println("Failed to read from DHT sensor!");
    return;
  }

  if (humidity > 70) {

    digitalWrite(relay_pin, LOW);
  } else {
    digitalWrite(relay_pin, HIGH);

  }

  Serial.print("%  Humidity: ");
  Serial.print(humidity);
  Serial.println("°C");

  // Ensure MQTT connection is active
  if (!client.connected()) {
    reconnect();
  }

  // Construct JSON payload with dynamic values
  String payload = "{";
  payload += "\"Humidity\":";
  payload += String(humidity, 1);  
  payload += "}";

  // Publish message to MQTT broker
  client.publish(mqtt_topic, payload.c_str());
  Serial.println("Published data: " + payload);

  // Wait before sending next data
  delay(3000);
}

void reconnect() {
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    if (client.connect("NodeMCU_Client")) {
      Serial.println("Reconnected!");
    } else {
      Serial.print("Failed, rc=");
      Serial.print(client.state());
      Serial.println(" Retrying in 5 seconds...");
      delay(3000);
    }
  }
}

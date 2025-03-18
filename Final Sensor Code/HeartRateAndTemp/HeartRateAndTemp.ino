#include <Wire.h>
#include "MAX30105.h"
#include "heartRate.h"
#include <WiFi.h>
#include <PubSubClient.h>

MAX30105 particleSensor;

const byte RATE_SIZE = 4;
byte rates[RATE_SIZE];
byte rateSpot = 0;
long lastBeat = 0;
float beatsPerMinute;
int beatAvg;

// Wi-Fi Credentials
const char* ssid = "iPhone";
const char* password = "24681012";

// MQTT Broker Info (Replace with your MQTT broker IP or public broker)
const char* mqtt_server = "172.20.10.5";
const int mqtt_port = 1883;
const char* mqtt_topic = "sensor/data";

// Create WiFi and MQTT Client objects
WiFiClient espClient;
PubSubClient client(espClient);

void connectWiFi() {
  Serial.print("Connecting to WiFi...");
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nConnected to WiFi!");
  Serial.print("IP Address: ");
  Serial.println(WiFi.localIP());
}

void connectMQTT() {
  while (!client.connected()) {
    Serial.print("Connecting to MQTT Broker...");
    if (client.connect("ESP32_Client")) {
      Serial.println("Connected to MQTT!");
    } else {
      Serial.print("Failed, rc=");
      Serial.print(client.state());
      Serial.println(" Retrying in 3 seconds...");
      delay(3000);
    }
  }
}

void setup() {
  Serial.begin(115200);

  connectWiFi();

  client.setServer(mqtt_server, mqtt_port);
  connectMQTT();

  //////////////////////////////////////////////////////////
  Serial.println("Initializing MAX30105...");

  if (!particleSensor.begin(Wire, I2C_SPEED_FAST)) {
    Serial.println("MAX30105 NOT found! Check wiring.");
    while (1)
      ;
  }

  Serial.println("Place your index finger on the sensor with steady pressure.");

  particleSensor.setup();
  particleSensor.setPulseAmplitudeRed(0x3F);
  particleSensor.setPulseAmplitudeGreen(0);
  particleSensor.enableDIETEMPRDY();
}

void loop() {

  if (!client.connected()) {
    connectMQTT();
  }
  client.loop();

  long irValue = particleSensor.getIR();

  if (checkForBeat(irValue)) {
    long delta = millis() - lastBeat;
    lastBeat = millis();

    beatsPerMinute = 60.0 / (delta / 1000.0);

    if (beatsPerMinute > 30 && beatsPerMinute < 200) {
      rates[rateSpot++] = (byte)beatsPerMinute;
      rateSpot %= RATE_SIZE;

      beatAvg = 0;
      for (byte i = 0; i < RATE_SIZE; i++) {
        beatAvg += rates[i];
      }
      beatAvg /= RATE_SIZE;
    }
  }





  static unsigned long lastTime = 0;
  if (millis() - lastTime > 1000) {
    lastTime = millis();

    if (irValue < 50000) {
      Serial.println("No finger detected!");
    } else {

      float temperatureC = particleSensor.readTemperature();
      // Construct JSON payload with dynamic values
      String payload = "{";
      payload += "\"HeartRate\":";
      payload += String(beatAvg);  // Ensure it's converted to a valid string
      payload += ",\"BodyTemp\":";
      payload += String(temperatureC, 1);
      payload += "}";


      // Publish message to MQTT broker
      client.publish(mqtt_topic, payload.c_str());
      Serial.println("Published data: " + payload);


      Serial.print("IR=");
      Serial.print(irValue);
      Serial.print(", BPM=");
      Serial.print(beatsPerMinute);
      Serial.print(", Avg BPM=");
      Serial.print(beatAvg);

      Serial.print(" | Temperature (C): ");
      Serial.print(temperatureC, 2);
      Serial.println(" °C");
    }
  }
}
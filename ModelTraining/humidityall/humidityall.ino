#include <DHT.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>

// ======== Wi-Fi Credentials ========
const char* ssid = "DiPhone";
const char* password = "123456789";

// ======== Server & MQTT Settings ========
const char* serverName = "http://172.20.10.5:5000/env-decision"; // AI API endpoint
const char* mqtt_server = "172.20.10.5";
const int mqtt_port = 1883;
const char* mqtt_topic = "poultry/light_status";

// ======== Pins ========
#define DHTPIN 14           // GPIO14 (D5)
#define DHTTYPE DHT11
#define MQ135_PIN A0

#define RELAY_FAN     12    // GPIO12 (D6)
#define RELAY_HEATER  13    // GPIO13 (D7)
#define RELAY_MISTER  15    // GPIO15 (D8)

// ======== Relay Logic ========
#define RELAY_ON LOW
#define RELAY_OFF HIGH

// ======== Globals ========
DHT dht(DHTPIN, DHTTYPE);
WiFiClient espClient;
PubSubClient mqttClient(espClient);

bool fanOn = false;
bool heaterOn = false;
bool misterOn = false;

unsigned long lastWifiAttempt = 0;
unsigned long lastMqttAttempt = 0;

// ======== Helper Functions ========

// Initialize relay pins and turn all relays OFF
void initRelays() {
  pinMode(RELAY_FAN, OUTPUT);
  pinMode(RELAY_HEATER, OUTPUT);
  pinMode(RELAY_MISTER, OUTPUT);
  digitalWrite(RELAY_FAN, RELAY_OFF);
  digitalWrite(RELAY_HEATER, RELAY_OFF);
  digitalWrite(RELAY_MISTER, RELAY_OFF);
}

// Control a relay by pin and boolean ON/OFF state
void controlRelay(uint8_t pin, bool state) {
  digitalWrite(pin, state ? RELAY_ON : RELAY_OFF);
}

// Attempt to reconnect Wi-Fi non-blocking (retry every 5 seconds)
void maintainWiFiConnection() {
  if (WiFi.status() != WL_CONNECTED && millis() - lastWifiAttempt > 5000) {
    Serial.println("Attempting Wi-Fi reconnect...");
    WiFi.disconnect();
    WiFi.begin(ssid, password);
    lastWifiAttempt = millis();
  }
}

// Attempt to reconnect MQTT non-blocking (retry every 5 seconds)
void maintainMQTTConnection() {
  if (!mqttClient.connected() && millis() - lastMqttAttempt > 5000) {
    Serial.print("Attempting MQTT connection...");
    String clientId = "ESP8266Client-" + String(random(0xffff), HEX);
    if (mqttClient.connect(clientId.c_str())) {
      Serial.println("✅ MQTT connected");
    } else {
      Serial.print("❌ MQTT failed, rc=");
      Serial.print(mqttClient.state());
      Serial.println(" -> will retry");
    }
    lastMqttAttempt = millis();
  }
}

// Read sensors, returns false if DHT read fails
bool readSensors(float &temperature, float &humidity, int &airQuality) {
  temperature = dht.readTemperature();
  humidity = dht.readHumidity();
  airQuality = analogRead(MQ135_PIN);

  if (isnan(temperature) || isnan(humidity)) {
    Serial.println("⚠️ Failed to read from DHT sensor!");
    return false;
  }
  return true;
}

// Publish sensor data via MQTT
void publishMQTTSensorData(float temp, float hum, int airQuality) {
  StaticJsonDocument<200> mqttDoc;
  mqttDoc["temperature"] = temp;
  mqttDoc["humidity"] = hum;
  mqttDoc["air_quality"] = airQuality;

  char mqttBuffer[256];
  serializeJson(mqttDoc, mqttBuffer);
  mqttClient.publish(mqtt_topic, mqttBuffer);
}

// Send sensor data to AI API and apply relay states based on response
void sendAIRequestAndApply(float temp, float hum, int airQuality) {
  if (WiFi.status() != WL_CONNECTED) return;

  HTTPClient http;
  WiFiClient wifiClient;
  http.begin(wifiClient, serverName);
  http.addHeader("Content-Type", "application/json");

  StaticJsonDocument<200> reqDoc;
  reqDoc["Temperature"] = temp;
  reqDoc["Humidity"] = hum;
  reqDoc["Air_Quality"] = airQuality;

  String requestBody;
  serializeJson(reqDoc, requestBody);

  int httpResponseCode = http.POST(requestBody);
  if (httpResponseCode == 200) {
    String response = http.getString();

    StaticJsonDocument<200> resDoc;
    DeserializationError error = deserializeJson(resDoc, response);

    if (!error) {
      String fanCmd = resDoc["fan"].as<String>();
      String heaterCmd = resDoc["heater"].as<String>();
      String misterCmd = resDoc["mister"].as<String>();

      // Update relays
      fanOn = (fanCmd == "ON");
      heaterOn = (heaterCmd == "ON");
      misterOn = (misterCmd == "ON");

      controlRelay(RELAY_FAN, fanOn);
      controlRelay(RELAY_HEATER, heaterOn);
      controlRelay(RELAY_MISTER, misterOn);

      Serial.println("🔁 AI Decision Applied:");
      Serial.print("  Fan: "); Serial.println(fanCmd);
      Serial.print("  Heater: "); Serial.println(heaterCmd);
      Serial.print("  Mister: "); Serial.println(misterCmd);
    } else {
      Serial.println("⚠️ Error parsing AI response");
    }
  } else {
    Serial.printf("❌ HTTP POST failed: %d\n", httpResponseCode);
  }

  http.end();
}

void setup() {
  Serial.begin(115200);
  dht.begin();

  initRelays();

  WiFi.begin(ssid, password);
  Serial.print("Connecting to Wi-Fi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\n✅ Wi-Fi connected");

  mqttClient.setServer(mqtt_server, mqtt_port);
  reconnectMQTT();

  Serial.println("✅ System initialized and ready.");
}

// Slightly modified reconnectMQTT to be non-blocking (called from loop)
void reconnectMQTT() {
  if (!mqttClient.connected()) {
    Serial.print("Attempting MQTT connection...");
    String clientId = "ESP8266Client-" + String(random(0xffff), HEX);
    if (mqttClient.connect(clientId.c_str())) {
      Serial.println("✅ MQTT connected");
    } else {
      Serial.print("❌ MQTT failed, rc=");
      Serial.print(mqttClient.state());
      Serial.println(" -> retrying in 5s");
      delay(5000);
    }
  }
}

void loop() {
  maintainWiFiConnection();

  if (!mqttClient.connected()) {
    maintainMQTTConnection();
  }
  mqttClient.loop();

  float temperature, humidity;
  int airQuality;

  if (!readSensors(temperature, humidity, airQuality)) {
    delay(5000);
    return;
  }

  Serial.printf("📊 Temp: %.1f°C | Humidity: %.1f%% | Air Quality: %d\n", temperature, humidity, airQuality);

  publishMQTTSensorData(temperature, humidity, airQuality);

  sendAIRequestAndApply(temperature, humidity, airQuality);

  Serial.print("Fan: "); Serial.print(fanOn ? "ON" : "OFF");
  Serial.print(" | Heater: "); Serial.print(heaterOn ? "ON" : "OFF");
  Serial.print(" | Mister: "); Serial.println(misterOn ? "ON" : "OFF");

  delay(1000);
}

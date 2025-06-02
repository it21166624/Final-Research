#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_TCS34725.h>
#include <BH1750.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>

// Wi-Fi credentials
const char* ssid = "DiPhone";
const char* password = "123456789";

// HTTP API endpoint
const char* serverName = "http://172.20.10.5:5000/light-decision";

// MQTT server
const char* mqtt_server = "172.20.10.5";
const int mqtt_port = 1883;
const char* mqtt_topic = "poultry/light_status";

// Relay pins
#define RELAY_WHITE 12  // D6
#define RELAY_RED 13    // D7
#define RELAY_ON LOW
#define RELAY_OFF HIGH

// Lux threshold for "daytime"
#define LUX_DAY_THRESHOLD 30
#define TOTAL_LIGHT_TARGET_SECONDS 57600  // 16 hours

// Sensors
BH1750 lightMeter;
Adafruit_TCS34725 tcs = Adafruit_TCS34725(TCS34725_INTEGRATIONTIME_300MS, TCS34725_GAIN_1X);
WiFiClient espClient;
PubSubClient client(espClient);

// Light state tracking
bool whiteOn = false;
bool redOn = false;
unsigned long totalArtificialLight = 0;
unsigned long totalNaturalLight = 0;
unsigned long lastDayStart = 0;

void reconnectMQTT() {
  while (!client.connected()) {
    Serial.print("Connecting to MQTT...");
    String clientId = "ESP8266Client-" + String(random(0xffff), HEX);
    if (client.connect(clientId.c_str())) {
      Serial.println("Connected.");
    } else {
      Serial.print("Failed, rc=");
      Serial.print(client.state());
      Serial.println(" Try again in 5 sec");
      delay(5000);
    }
  }
}

void setup() {
  Serial.begin(115200);
  Wire.begin();

  pinMode(RELAY_WHITE, OUTPUT);
  pinMode(RELAY_RED, OUTPUT);
  digitalWrite(RELAY_WHITE, RELAY_OFF);
  digitalWrite(RELAY_RED, RELAY_OFF);

  WiFi.begin(ssid, password);
  Serial.print("Connecting to Wi-Fi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\n✅ Wi-Fi connected.");

  client.setServer(mqtt_server, mqtt_port);
  reconnectMQTT();

  while (!lightMeter.begin()) {
    Serial.println("BH1750 init failed... retrying.");
    delay(1000);
  }

  while (!tcs.begin()) {
    Serial.println("TCS34725 init failed... retrying.");
    delay(1000);
  }

  lastDayStart = millis();
  Serial.println("System ready.");
}

void loop() {
  float lux = lightMeter.readLightLevel();
  uint16_t r, g, b, c;
  tcs.getRawData(&r, &g, &b, &c);

  // Track light exposure every second
  static unsigned long lastCheck = millis();
  if (millis() - lastCheck >= 1000) {
    lastCheck = millis();
    if (lux >= LUX_DAY_THRESHOLD) {
      totalNaturalLight += 1000;
    }
    if (whiteOn || redOn) {
      totalArtificialLight += 1000;
    }
  }

  // Reset after 24 hours
  if (millis() - lastDayStart >= 86400000UL) {
    Serial.println("🔁 Daily Reset");
    totalNaturalLight = 0;
    totalArtificialLight = 0;
    lastDayStart = millis();
  }

  // Calculate total light duration
  unsigned long totalLightToday = totalNaturalLight + totalArtificialLight;
  bool targetMet = totalLightToday >= (TOTAL_LIGHT_TARGET_SECONDS * 1000UL);
  bool isNight = lux < LUX_DAY_THRESHOLD;

  if (!targetMet && isNight) {
    // Call AI API only if we're still under target and it's night
    Serial.println("🌙 Nighttime + under target → Checking AI decision...");

    if (WiFi.status() == WL_CONNECTED) {
      HTTPClient http;
      WiFiClient client;
      http.begin(client, serverName);
      http.addHeader("Content-Type", "application/json");

      // Prepare JSON
      String jsonPayload;
      StaticJsonDocument<200> doc;
      doc["lux"] = lux;
      doc["red"] = r;
      doc["green"] = g;
      doc["blue"] = b;
      serializeJson(doc, jsonPayload);
      Serial.print("📤 Sending JSON Payload: ");
      Serial.println(jsonPayload);

      int httpResponseCode = http.POST(jsonPayload);
      if (httpResponseCode == 200) {
        String response = http.getString();
        Serial.print("📡 API Response: ");
        Serial.println(response);

        StaticJsonDocument<100> respDoc;
        DeserializationError err = deserializeJson(respDoc, response);
        if (!err) {
          String decision = respDoc["light_decision"];
          decision.trim();
          Serial.print("🧠 AI Decision: ");
          Serial.println(decision);

          // Relay logic
          whiteOn = (decision == "WHITE" || decision == "BOTH");
          redOn = (decision == "RED" || decision == "BOTH");

          digitalWrite(RELAY_WHITE, whiteOn ? RELAY_ON : RELAY_OFF);
          digitalWrite(RELAY_RED, redOn ? RELAY_ON : RELAY_OFF);
        } else {
          Serial.println("❌ Failed to parse JSON.");
        }
      } else {
        Serial.print("❌ HTTP error: ");
        Serial.println(httpResponseCode);
      }

      http.end();
    }

  } else {
    // Daylight or target met → turn OFF all relays
    if (whiteOn || redOn) {
      Serial.println("☀️ Daytime or Target Met → Lights OFF.");
    }
    whiteOn = false;
    redOn = false;
    digitalWrite(RELAY_WHITE, RELAY_OFF);
    digitalWrite(RELAY_RED, RELAY_OFF);
  }

  // Reporting
  unsigned long totalSeconds = (totalLightToday) / 1000;
  unsigned long totalMinutes = totalSeconds / 60;
  unsigned long totalHours = totalMinutes / 60;

  Serial.print("🕓 Light Time → Natural: ");
  Serial.print(totalNaturalLight / 1000);
  Serial.print("s | Artificial: ");
  Serial.print(totalArtificialLight / 1000);
  Serial.print("s | Total: ");
  Serial.print(totalHours);
  Serial.print("h ");
  Serial.print(totalMinutes % 60);
  Serial.print("m ");
  Serial.print(totalSeconds % 60);
  Serial.println("s");

  Serial.print("⚡ White Light: ");
  Serial.print(whiteOn ? "ON" : "OFF");
  Serial.print(" | Red Light: ");
  Serial.println(redOn ? "ON" : "OFF");

  // MQTT Payload
  String payload = "{\"Light_Hours_Sec\": ";
  payload += totalSeconds;
  payload += ", \"white_on\": ";
  payload += whiteOn ? "true" : "false";
  payload += ", \"red_on\": ";
  payload += redOn ? "true" : "false";
  payload += "}";

  if (!client.connected()) reconnectMQTT();
  client.loop();
  client.publish(mqtt_topic, payload.c_str());

  yield();  // prevent watchdog reset
  delay(1000);
}

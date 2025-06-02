#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_TCS34725.h>
#include <BH1750.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>

// Wi-Fi credentials
const char* ssid = "Chana’s iPhone";
const char* password = "123456789";

// API endpoint
const char* serverName = "http://172.20.10.3:5000/light-decision";  // Replace with your actual IP

// Sensors
BH1750 lightMeter;
Adafruit_TCS34725 tcs = Adafruit_TCS34725(TCS34725_INTEGRATIONTIME_300MS, TCS34725_GAIN_1X);

// Relay pins
#define RELAY_WHITE 12  // D6
#define RELAY_RED 13    // D7
#define RELAY_ON LOW
#define RELAY_OFF HIGH

#define LUX_DAY_THRESHOLD 30
#define TOTAL_LIGHT_TARGET_SECONDS 57600

// Tracking
unsigned long totalArtificialLight = 0;
unsigned long totalNaturalLight = 0;
unsigned long lastDayStart = 0;
bool whiteOn = false;
bool redOn = false;

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

  static unsigned long lastCheck = millis();
  if (millis() - lastCheck >= 1000) {
    lastCheck = millis();
    if (lux >= LUX_DAY_THRESHOLD) totalNaturalLight += 1000;
    if (whiteOn || redOn) totalArtificialLight += 1000;
  }

  unsigned long totalLightToday = totalNaturalLight + totalArtificialLight;

  if (millis() - lastDayStart >= 86400000UL) {
    Serial.println("Daily reset.");
    totalNaturalLight = 0;
    totalArtificialLight = 0;
    lastDayStart = millis();
  }

  if (totalLightToday < TOTAL_LIGHT_TARGET_SECONDS * 1000UL) {
    if (lux < LUX_DAY_THRESHOLD) {
      Serial.println("Nighttime + short light: Getting AI light control from API...");

      if (WiFi.status() == WL_CONNECTED) {
        HTTPClient http;
        // http.begin(serverName);
        WiFiClient client;
        http.begin(client, serverName);
        http.addHeader("Content-Type", "application/json");

        // Prepare JSON payload
        String payload;
        StaticJsonDocument<200> doc;
        doc["lux"] = lux;
        doc["red"] = r;
        doc["green"] = g;
        doc["blue"] = b;
        serializeJson(doc, payload);

        int httpResponseCode = http.POST(payload);
        if (httpResponseCode == 200) {
          String response = http.getString();
          StaticJsonDocument<100> respDoc;
          DeserializationError err = deserializeJson(respDoc, response);

          if (!err) {
            String decision = respDoc["light_decision"];
            decision.trim();
            Serial.print("AI Decision: ");
            Serial.println(decision);

            // Relay logic
            whiteOn = (decision == "WHITE" || decision == "BOTH");
            redOn = (decision == "RED" || decision == "BOTH");

            digitalWrite(RELAY_WHITE, whiteOn ? RELAY_ON : RELAY_OFF);
            digitalWrite(RELAY_RED, redOn ? RELAY_ON : RELAY_OFF);
          } else {
            Serial.println("⚠️ Failed to parse JSON response.");
          }
        } else {
          Serial.print("HTTP Error: ");
          Serial.println(httpResponseCode);
        }

        http.end();
      }
    } else {
      // Daylight sufficient
      whiteOn = false;
      redOn = false;
      digitalWrite(RELAY_WHITE, RELAY_OFF);
      digitalWrite(RELAY_RED, RELAY_OFF);
      Serial.println("Daylight sufficient: Lights OFF.");
    }
  } else {
    whiteOn = false;
    redOn = false;
    digitalWrite(RELAY_WHITE, RELAY_OFF);
    digitalWrite(RELAY_RED, RELAY_OFF);
    Serial.println("Target met: Lights OFF.");
  }

  // Print status
  // Serial.print("Natural (s): ");
  // Serial.print(totalNaturalLight / 1000);
  // Serial.print(" | Artificial (s): ");
  // Serial.print(totalArtificialLight / 1000);
  // Serial.print(" | Total (h): ");
  // Serial.println((totalNaturalLight + totalArtificialLight) / 3600000.0, 2);

  // Serial.print("White Light: ");
  // Serial.print(whiteOn ? "ON" : "OFF");
  // Serial.print(" | Red Light: ");
  // Serial.println(redOn ? "ON" : "OFF");

  unsigned long totalSeconds = (totalNaturalLight + totalArtificialLight) / 1000;
  unsigned long totalMinutes = totalSeconds / 60;
  unsigned long totalHours = totalMinutes / 60;

  Serial.print("Natural Light: ");
  Serial.print(totalNaturalLight / 1000);
  Serial.print(" s | Artificial Light: ");
  Serial.print(totalArtificialLight / 1000);
  Serial.println(" s");

  Serial.print("Total Light Time: ");
  Serial.print(totalHours);
  Serial.print(" h ");
  Serial.print(totalMinutes % 60);
  Serial.print(" m ");
  Serial.print(totalSeconds % 60);
  Serial.println(" s");

  Serial.print("White Light: ");
  Serial.print(whiteOn ? "ON" : "OFF");
  Serial.print(" | Red Light: ");
  Serial.println(redOn ? "ON" : "OFF");


  yield();
  delay(1000);  // 10 seconds
}

#include <DHT.h>

#define LED_PIN 2      // GPIO pin for LED
#define DHT_PIN 15     // GPIO pin for DHT11 sensor
#define DHT_TYPE DHT11 // Specify DHT11 type

DHT dht(DHT_PIN, DHT_TYPE);  // Create DHT object

void setup() {
    Serial.begin(9600);           // Initialize serial communication
    pinMode(LED_PIN, OUTPUT);     // Set LED pin as output
    dht.begin();                  // Initialize DHT sensor
}

void loop() {
    // Get temperature and humidity from the DHT11 sensor
    float humidity = dht.readHumidity();
    float temperature = dht.readTemperature(); // Read temperature in Celsius

    // Check if any read failed and exit early (to avoid printing bad data)
    if (isnan(humidity) || isnan(temperature)) {
        Serial.println("Failed to read from DHT sensor!");
        return;
    }

    // Send sensor data
    Serial.print("Temperature: ");
    Serial.print(temperature);
    Serial.print(" °C, Humidity: ");
    Serial.print(humidity);
    Serial.println(" %");

    // Check for incoming commands to control the LED
    if (Serial.available() > 0) {
        char command = Serial.read();
        if (command == '1') {
            digitalWrite(LED_PIN, HIGH);  // Turn LED on
        } else if (command == '0') {
            digitalWrite(LED_PIN, LOW);   // Turn LED off
        }
    }

    delay(1000);  // Send data every second
}

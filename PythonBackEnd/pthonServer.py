import pandas as pd
import psycopg2
import pickle
import numpy as np
from flask import Flask, request, jsonify
from sklearn.ensemble import RandomForestRegressor
from sklearn.preprocessing import LabelEncoder
import joblib
from xgboost import XGBClassifier
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LinearRegression

app = Flask(__name__)

# ========================= Config ===========================

DB_CONFIG = {
    "host": "localhost",
    "database": "egg_monitoring",
    "user": "postgres",
    "password": "admin123"
}

MODEL_FILE = "egg_model.pkl"
LABEL_ENCODER_FILE = "label_encoderold.pkl"
LIGHT_MODEL_FILE = "light_control_model.pkl"
ENV_MODEL_FILE = "environmental_data.pkl"
STRESS_DATASET_FILE = "sample_hen_stress_data.csv"
STRESS_MODEL_FILE = "stress_prediction_model.pkl"

# ========================= Globals ==========================

model = None
label_encoder = None
light_model = None
env_model = None
stress_model = None

# ========================= Egg Prediction Logic ===========================

def fetch_sensor_data():
    query = """
    SELECT "Temperature", "Humidity", "Light_Hours", "Hen_Age_weeks", "Feed_Quantity", "Health_Status", "Hen_Count", "Egg_count"
    FROM hen_production;
    """
    try:
        with psycopg2.connect(**DB_CONFIG) as conn:
            return pd.read_sql_query(query, conn)
    except Exception as e:
        print(f"Database Error: {e}")
        return pd.DataFrame()

def train_model():
    global model, label_encoder
    data = fetch_sensor_data()
    if data.empty:
        print("No data available.")
        return
    label_encoder = LabelEncoder()
    data["Health_Status"] = label_encoder.fit_transform(data["Health_Status"])
    X = data[['Temperature', 'Humidity', 'Light_Hours', 'Hen_Age_weeks', 'Feed_Quantity', 'Health_Status', 'Hen_Count']]
    y = data['Egg_count']
    model = LinearRegression()
    model.fit(X, y)
    with open(MODEL_FILE, "wb") as f:
        pickle.dump(model, f)
    with open(LABEL_ENCODER_FILE, "wb") as f:
        pickle.dump(label_encoder, f)

def load_model():
    global model, label_encoder, light_model, env_model
    try:
        with open(MODEL_FILE, "rb") as f:
            model = pickle.load(f)
        with open(LABEL_ENCODER_FILE, "rb") as f:
            label_encoder = pickle.load(f)
        print("Egg prediction model and label encoder loaded.")
    except Exception as e:
        print(f"Training model from scratch... Error: {e}")
        train_model()

    try:
        with open(LIGHT_MODEL_FILE, "rb") as f:
            light_model = pickle.load(f)
        print("Light control model loaded.")
    except Exception as e:
        print(f"Light model not found or failed to load: {e}")

    try:
        with open(ENV_MODEL_FILE, "rb") as f:
            env_model = pickle.load(f)
        print("ENV control model loaded.")
    except Exception as e:
        print(f"ENV model not found or failed to load: {e}")

def predict_egg_count(input_data):
    global model
    input_array = np.array(input_data).reshape(1, -1)
    return model.predict(input_array)[0]

# ========================= Stress Prediction Logic =======================

def load_stress_model():
    global stress_model
    try:
        stress_model = joblib.load(STRESS_MODEL_FILE)
        print("Stress model loaded.")
    except Exception as e:
        print(f"Stress model not found or failed to load: {e}")

def train_stress_model():
    global stress_model

    try:
        df = pd.read_csv(STRESS_DATASET_FILE)
        X = df[['Temperature', 'Humidity', 'FeedIntakePerHen', 'WaterIntakePerHen', 'AirQuality',
                'Lighting', 'CageDensity', 'Vocalization', 'BodyTemperature', 'Heartbeat']]
        y = df['StressLevel']  # 0 = Low, 1 = Medium, 2 = High
        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
        stress_model = XGBClassifier(n_estimators=100)
        stress_model.fit(X_train, y_train)
        joblib.dump(stress_model, STRESS_MODEL_FILE)
        print("Stress Prediction Model trained and saved successfully.")
        return True
    except Exception as e:
        print(f"Error training stress model: {e}")
        return False

# ========================= API Endpoints ===========================

@app.route("/predict", methods=["POST"])
def predict():
    global label_encoder
    data = request.get_json()
    try:
        if model is None:
            load_model()
        if label_encoder is None:
            return jsonify({"error": "Label encoder not loaded."}), 500

        temp = data.get("Temperature")
        humidity = data.get("Humidity")
        light = data.get("Light_Hours")
        henAge = data.get("Hen_Age_weeks")
        health = data.get("Health_Status")
        feed_quantity = data.get("Feed_Quantity")
        henCount = data.get("Hen_Count")

        if None in [temp, humidity, light, henAge, health, feed_quantity, henCount]:
            return jsonify({"error": "Missing input data"}), 400

        if health not in label_encoder.classes_:
            return jsonify({"error": "Invalid Health_Status"}), 400

        health_encoded = label_encoder.transform([health])[0]
        result = predict_egg_count([temp, humidity, light, henAge, feed_quantity, health_encoded, henCount])
        return jsonify({"predicted_egg_count": result})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route("/predict_stress", methods=["POST"])
def predict_stress():
    global stress_model
    try:
        if stress_model is None:
            print("Stress model not loaded. Training now...")
            trained = train_stress_model()
            if not trained:
                return jsonify({"error": "Failed to train stress model."}), 500

        data = request.json

        features = np.array([
            data["Temperature"], data["Humidity"], data["FeedIntakePerHen"],
            data["WaterIntakePerHen"], data["AirQuality"], data["Lighting"], data["CageDensity"],
            data["Vocalization"], data["BodyTemperature"], data["Heartbeat"]
        ]).reshape(1, -1)

        prediction = stress_model.predict(features)[0]
        return jsonify({"predicted_stress": int(prediction)})

    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route("/light-decision", methods=["POST"])
def light_decision():
    global light_model
    try:
        data = request.get_json()
        lux = data.get("lux")
        r = data.get("red")
        g = data.get("green")
        b = data.get("blue")

        if None in [lux, r, g, b]:
            return jsonify({"error": "Missing lux or RGB inputs."}), 400

        if light_model is None:
            return jsonify({"error": "Light control model not loaded."}), 500

        features = np.array([[lux, r, g, b]])
        decision = light_model.predict(features)[0]
        return jsonify({"light_decision": decision})

    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route("/env-decision", methods=["POST"])
def env_decision():
    global env_model
    try:
        raw_data = request.get_json()

        t = raw_data.get("Temperature") or raw_data.get("temperature")
        h = raw_data.get("Humidity") or raw_data.get("humidity")
        a = raw_data.get("Air_Quality") or raw_data.get("air_quality")

        if None in [t, h, a]:
            return jsonify({"error": "Missing inputs."}), 400

        if env_model is None:
            return jsonify({"error": "env control model not loaded."}), 500

        features = pd.DataFrame([[t, h, a]], columns=["Temperature", "Humidity", "Air_Quality"])

        decision = env_model.predict(features)
        device_names = ['fan', 'heater', 'mister']
        statuses = dict(zip(device_names, list(decision[0])))
        return jsonify(statuses)

    except Exception as e:
        import traceback
        print("Exception occurred:", traceback.format_exc())
        return jsonify({"error": str(e)}), 500

# ========================= Main ============================================

if __name__ == "__main__":
    load_model()
    load_stress_model()
    app.run(host="0.0.0.0", port=5000)

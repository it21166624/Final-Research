import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.ensemble import GradientBoostingRegressor
from sklearn.metrics import mean_squared_error
from sklearn.preprocessing import OneHotEncoder
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
import joblib  # For saving the model

# Load the dataset
data = pd.read_csv(r"D:\Research\JN\JN\Book1.csv")

# Check for missing values
print(data.isnull().sum())

# Define the features and target variable
X = data.drop(columns=['Egg_count'])
y = data['Egg_count']

# Identify categorical and numerical columns
categorical_cols = ['Health_Status']
numerical_cols = X.columns.difference(categorical_cols)

# Preprocessing for numerical data: no transformation needed
# Preprocessing for categorical data: one-hot encode the categorical columns
preprocessor = ColumnTransformer(
    transformers=[('num', 'passthrough', numerical_cols),
                  ('cat', OneHotEncoder(), categorical_cols)])

# Split the data into training and testing sets
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

# Create and train the model pipeline
model = Pipeline(steps=[
    ('preprocessor', preprocessor),
    ('regressor', GradientBoostingRegressor())
])

# Train the model
model.fit(X_train, y_train)

# Make predictions on the test set
y_pred = model.predict(X_test)

# Calculate the mean squared error
mse = mean_squared_error(y_test, y_pred)
print(f"Mean Squared Error: {mse}")

# Save the trained model as a .pkl file
model_path = r'C:\Users\it21176906\Documents\egg_yield_predictor.pkl'
joblib.dump(model, model_path)
print(f"Model saved successfully to {model_path}")

# Function to optimize temperature for maximum egg production with a range
def optimize_temperature_range(model, min_temp, max_temp, other_factors, margin=2):
    # Generate a range of temperatures
    temperatures = np.linspace(min_temp, max_temp, 100)  # Generate 100 temperature values
    predictions = []

    # Loop through temperature values and predict egg count
    for temp in temperatures:
        # Prepare input data with constant other factors
        new_data = {
            'Temperature': [temp],
            'Humidity': [other_factors['Humidity']],
            'Light_Hours': [other_factors['Light_Hours']],
            'Hen_Age_weeks': [other_factors['Hen_Age_weeks']],
            'Feed_Quantity': [other_factors['Feed_Quantity']],
            'Health_Status': [other_factors['Health_Status']],
            'Hen_Count': [other_factors['Hen_Count']]
        }
        new_data_df = pd.DataFrame(new_data)

        # Predict egg count
        prediction = model.predict(new_data_df)[0]
        predictions.append(prediction)

    # Find the temperature corresponding to the maximum egg count
    max_egg_count_idx = np.argmax(predictions)
    optimal_temp = temperatures[max_egg_count_idx]
    max_egg_count = np.max(predictions)

    # Define a range around the optimal temperature (with a given margin)
    lower_bound = max(min_temp, optimal_temp - margin)
    upper_bound = min(max_temp, optimal_temp + margin)

    return (lower_bound, upper_bound), max_egg_count

# Example usage of the function
other_factors = {
    'Humidity': 65.1,  # example humidity
    'Light_Hours': 15.9,  # example light hours
    'Hen_Age_weeks': 17,  # example hen age
    'Feed_Quantity': 0.075,  # example feed quantity
    'Health_Status': 'Healthy',  # example health status
    'Hen_Count': 2021  # example hen count
}

# Call the optimization function to find the optimal temperature range
optimal_temp_range, max_egg_count = optimize_temperature_range(model, 20, 35, other_factors)

print(f"Optimal Temperature Range for Maximum Egg Production: {optimal_temp_range[0]:.2f}°C - {optimal_temp_range[1]:.2f}°C")
print(f"Maximum Predicted Egg Count: {max_egg_count:.2f}")

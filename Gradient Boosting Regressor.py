import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.ensemble import GradientBoostingRegressor
from sklearn.metrics import mean_squared_error
from sklearn.preprocessing import OneHotEncoder
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
import joblib  # Import joblib for saving the model

# Load the dataset
data = pd.read_csv(r"D:\Research\JN\JN\Book1.csv")

# Display the first few rows of the dataset
print(data.head())

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
    transformers=[
        ('num', 'passthrough', numerical_cols),
        ('cat', OneHotEncoder(), categorical_cols)
    ])

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

# Display the feature importances
feature_importances = model.named_steps['regressor'].feature_importances_
feature_names = numerical_cols.tolist() + model.named_steps['preprocessor'].named_transformers_['cat'].get_feature_names_out(categorical_cols).tolist()
feature_importances_series = pd.Series(feature_importances, index=feature_names)
print(feature_importances_series.sort_values(ascending=False))

# Function to make predictions on new data
def predict_new_data(new_data):
    new_data_df = pd.DataFrame(new_data)
    predictions = model.predict(new_data_df)
    return predictions

# Example of how to use the predict_new_data function with new data
new_data_example = {
    'Temperature': [28.1],
    'Humidity': [65.1],
    'Light_Hours': [15.9],
    'Hen_Age_weeks': [17],
    'Feed_Quantity': [0.075],
    'Health_Status': ['Healthy'],
    'Hen_Count': [2021]
}

new_predictions = predict_new_data(new_data_example)
print(f"Predictions for new data: {new_predictions}")

# Save the trained model as a .pkl file in the specified path
model_path = r'C:\Users\it21176906\Documents\egg_yield_predictor.pkl'
joblib.dump(model, model_path)

# Print success message
print(f"Model saved successfully to {model_path}")

# To load the model back in the future
# loaded_model = joblib.load(model_path)

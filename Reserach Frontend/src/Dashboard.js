import React, { useState, useEffect } from "react";
import styled from "styled-components";
import Lottie from "lottie-react";
import AdapterDateFns from "@mui/lab/AdapterDateFns";

import { DemoContainer } from "@mui/x-date-pickers/internals/demo";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";

import axios from "axios";

// ✅ Import Lottie Animation JSON files
import humidityAnimation from "./animations/humidity.json";
import lightAnimation from "./animations/light.json";
import temperatureAnimation from "./animations/temperature.json";
import heartRateAnimation from "./animations/heartbeat.json";
import bodyTempAnimation from "./animations/bodytemp.json";
import henCountAnimation from "./animations/hen.json";
import ageAnimation from "./animations/age.json";
import dayjs from "dayjs";

// ✅ Styled Components (Matching Background)
const DashboardContainer = styled.div`
  position: relative;
  background: url("/EggBackground.jpg") no-repeat center center;
  background-size: cover;
  filter: brightness(90%);
  color: white;
  padding: 20px;
  text-align: center;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-end; /* Moves the card to the bottom */

  /* Dark Overlay */
  &::before {
    content: "";
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5); /* Adjust darkness */
    z-index: -1;
  }
`;

const StatsCard = styled.div`
  background: rgba(185, 174, 174, 0.2);
  backdrop-filter: blur(60px);
  padding: 10px; /* Reduced padding */
  width: 95vw;
  max-width: 100%;
  text-align: center;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.3);
  border: 1px solid rgba(255, 255, 255, 0.3);
  display: flex;
  justify-content: space-around;
  align-items: center;
  margin-bottom: 300px; /* Reduced margin to adjust spacing */
  height: auto; /* Let it adjust based on content */
  min-height: 100px; /* Ensures it's not too small */
`;

const StatItem = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 10px;
  flex: 1;
  border-right: 2px solid rgba(255, 255, 255, 0.5);
  &:last-child {
    border-right: none;
  }
`;

const LottieIcon = styled(Lottie)`
  width: 60px;
  height: 60px;
  margin-bottom: 5px;
`;

const Badge = styled.span`
  padding: 6px 12px;
  font-size: 14px;
  font-weight: bold;
  color: ${(props) => (props.type === "normal" ? "#006400" : "#8b0000")};
  background: ${(props) => (props.type === "normal" ? "#d4edda" : "#f8d7da")};
  border-radius: 4px;
  margin-top: 8px;
  box-shadow: 0px 3px 8px rgba(0, 0, 0, 0.1);
  display: inline-block;
`;

const SyncIndicator = styled.div`
  position: absolute;
  top: 20px;
  right: 20px;
  display: flex;
  align-items: center;
  // padding: 10px;
  border-radius: 8px;
`;

const SyncImage = styled.img`
  width: 40px;
  height: 40px;
  margin-right: 10px;
`;

const SyncText = styled.span`
  font-size: 14px;
  font-weight: bold;
`;

const ExtraContainer = styled.div`
  position: absolute;
  top: 30%;
  right: 20px;
  width: 435px;
  display: flex;
  flex-direction: row;
  gap: 10px;
  align-items: center;
`;

const ExtraCard = styled.div`
  background: rgba(185, 174, 174, 0.2);
  backdrop-filter: blur(60px);
  padding: 20px;
  width: 240px;
  height: 120px;
  text-align: center;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.3);
  border: 1px solid rgba(255, 255, 255, 0.3);
  display: flex;
  flex-direction: column;
  justify-content: center;
`;

const PredictButton = styled.button`
  position: absolute;
  bottom: 50px;
  right: 20px;
  background-color: #007bff;
  color: white;
  border: none;
  padding: 10px 20px;
  font-size: 16px;
  font-weight: bold;
  cursor: pointer;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.3);
  transition: background-color 0.3s ease;

  &:hover {
    background-color: #0056b3;
  }
`;

const ModalOverlay = styled.div`
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
`;

const ModalContainer = styled.div`
  background: white;
  width: 300px;
  padding: 20px;
  border-radius: 8px;
  text-align: center;
  box-shadow: 0px 4px 8px rgba(168, 65, 65, 0.3);
  position: relative;
  color: black; /* ✅ Ensures text is visible */
`;

const CloseButton = styled.button`
  position: absolute;
  top: 10px;
  right: 10px;
  background: red;
  color: white;
  border: none;
  padding: 5px 10px;
  cursor: pointer;
  border-radius: 5px;
`;

const SubmitButton = styled.button`
  background: #007bff;
  color: white;
  border: none;
  padding: 10px;
  width: 100%;
  cursor: pointer;
  margin-top: 10px;
  border-radius: 5px;
  &:hover {
    background: #0056b3;
  }
`;

const Dashboard = () => {
  const [sensorData, setSensorData] = useState();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [value, setValue] = React.useState(dayjs("2025-03-18"));
  const [prediction, setPrediction] = useState(null);
  const [hensage, setHenAge] = useState([]);

  const fetchSensorData = async () => {
    try {
      const response = await axios.get(
        "https://localhost:7039/api/sensordata/liveData"
      );
      setSensorData(response.data[0]);
    } catch (error) {
      console.error("Error fetching sensor data:", error);
    }
  };

  useEffect(() => {
    fetchSensorData();
    const interval = setInterval(fetchSensorData, 1000);

    return () => clearInterval(interval);
  }, []);

  const handlePredict = () => {
    setIsModalOpen(true);
  };

  const handleSubmit = async () => {
    //   let data = {
    //     Temperature: sensorData?.temperature ?? 0,
    //     Humidity: sensorData?.humidity ?? 0,
    //     Light_Hours: sensorData?.light_Hours ?? 0,
    //     Hen_Age_weeks: hensage ?? 0,
    //     Feed_Quantity: sensorData?.feed_Quantity ?? 0,
    //     Health_Status: sensorData?.health_Status ?? "Unknown",
    //     Hen_Count: sensorData?.hen_Count ?? 0,
    //   };

    let data = JSON.stringify({
      Temperature: sensorData?.temperature ?? 0,
      Humidity: sensorData?.humidity ?? 0,
      Light_Hours: sensorData?.light_Hours ?? 0,
      Hen_Age_weeks: hensage ?? 0,
      Feed_Quantity: sensorData?.feed_Quantity ?? 0,
      Health_Status: "Healthy",
      Hen_Count: sensorData?.hen_Count ?? 0,
    });

    let config = {
      method: "post",
      maxBodyLength: Infinity,
      url: "https://localhost:7039/api/Prediction/EggPredict",
      headers: {
        "Content-Type": "application/json",
      },
      data: data,
    };

    axios
      .request(config)
      .then((response) => {
        console.log(JSON.stringify(response.data));
        setPrediction(response.data?.predicted_egg_count);
      })
      .catch((error) => {
        console.log(error);
      });
    // try {
    //   if (!sensorData) {
    //     console.error("Sensor data is undefined. Cannot proceed.");
    //     return;
    //   }

    //   let data = {
    //     Temperature: sensorData?.temperature ?? 0,
    //     Humidity: sensorData?.humidity ?? 0,
    //     Light_Hours: sensorData?.light_Hours ?? 0,
    //     Hen_Age_weeks: hensage ?? 0,
    //     Feed_Quantity: sensorData?.feed_Quantity ?? 0,
    //     Health_Status: sensorData?.health_Status ?? "Unknown",
    //     Hen_Count: sensorData?.hen_Count ?? 0,
    //   };

    //   const response = await axios.post(
    //     "http://localhost:7039/api/Prediction/EggPredict", // Fixed URL
    //     data
    //   );

    //   if (response.data && response.data.length > 0) {
    //     setPrediction(response.data[0]); // Ensure valid response
    //   } else {
    //     console.warn("Unexpected API response:", response.data);
    //   }
    // } catch (error) {
    //   console.error("Error fetching sensor data:", error);
    // }
  };

  useEffect(() => {
    const fetchAndUpdateHens = async () => {
      try {
        // Ensure sensorData and Born_Date exist
        if (!sensorData?.born_Date) {
          console.error("Born_Date is missing or invalid:", sensorData);
          return;
        }

        // Convert value to a Date (if value is dayjs)
        const currentDate = value?.toDate();
        if (!currentDate || isNaN(currentDate.getTime())) {
          console.error("Invalid current date:", currentDate);
          return;
        }

        // Convert Born_Date to a JavaScript Date
        const bornDate = new Date(sensorData.born_Date);
        if (isNaN(bornDate.getTime())) {
          console.error("Invalid Born_Date:", sensorData.born_Date);
          return;
        }

        // Calculate Hen_Age_weeks
        const updatedHens =
          Math.round(
            ((currentDate - bornDate) / (7 * 24 * 60 * 60 * 1000)) * 100
          ) / 100;

        console.log("updatedHens:", updatedHens);
        setHenAge(updatedHens);
      } catch (error) {
        console.error("Error updating hen ages:", error);
      }
    };

    if (sensorData) {
      fetchAndUpdateHens();
    }
  }, [value]);

  return (
    <DashboardContainer>
      <SyncIndicator>
        <SyncImage src="/sync.gif" alt="Syncing..." />
        <SyncText>Synchronizing...</SyncText>
      </SyncIndicator>

      {/* <ExtraContainer>
        <ExtraCard>
          <h3>Extra Card 1</h3>
          <p>Additional Data Here</p>
        </ExtraCard>
        <ExtraCard>
          <h3>Extra Card 2</h3>
          <p>Additional Data Here</p>
        </ExtraCard>
      </ExtraContainer> */}

      <StatsCard>
        <StatItem>
          <LottieIcon animationData={humidityAnimation} loop />
          <h3>Humidity</h3>
          <p>{sensorData?.humidity}%</p>
          {/* <Badge type="normal">Normal</Badge> */}
        </StatItem>

        <StatItem>
          <LottieIcon animationData={lightAnimation} loop />
          <h3>Light Level</h3>
          <p>
            {sensorData?.light_Hours ? Math.ceil(sensorData.light_Hours) : 0}Hrs
          </p>
          {/* <Badge type="normal">Normal</Badge> */}
        </StatItem>

        <StatItem>
          <LottieIcon animationData={temperatureAnimation} loop />
          <h3>Temperature</h3>
          <p>{sensorData?.temperature}°C</p>
          {/* <Badge type="normal">Normal</Badge> */}
        </StatItem>

        <StatItem>
          <LottieIcon animationData={heartRateAnimation} loop />
          <h3>Heart Rate</h3>
          <p>{sensorData?.heartRate} bpm</p>
          {/* <Badge type="warning">Warning</Badge> */}
        </StatItem>

        <StatItem>
          <LottieIcon animationData={bodyTempAnimation} loop />
          <h3>Body Temperature</h3>
          <p>{sensorData?.bodyTemp}°C</p>
          {/* <Badge type="warning">Warning</Badge> */}
        </StatItem>

        <StatItem>
          <LottieIcon animationData={henCountAnimation} loop />
          <h3>Hen Count</h3>
          <p>{sensorData?.hen_Count}</p>
          {/* <Badge type="normal">Stable</Badge> */}
        </StatItem>

        <StatItem>
          <LottieIcon animationData={ageAnimation} loop />
          <h3>Hen Age (Weeks)</h3>
          <p>{sensorData?.hen_Age_weeks} weeks</p>
          {/* <Badge type="normal">Growing</Badge> */}
        </StatItem>
      </StatsCard>

      <PredictButton onClick={handlePredict}>Predict</PredictButton>

      {isModalOpen && (
        <ModalOverlay>
          <ModalContainer>
            <CloseButton onClick={() => setIsModalOpen(false)}>X</CloseButton>
            <h3>Select Date</h3>

            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <DemoContainer components={["DatePicker"]}>
                <DatePicker
                  label="Basic date picker"
                  value={value}
                  onChange={(newValue) => setValue(newValue)}
                />
              </DemoContainer>
            </LocalizationProvider>
            <SubmitButton onClick={handleSubmit}>Submit</SubmitButton>
            {prediction && (
              <div style={{ marginTop: "20px", fontWeight: "bold" }}>
                <h4>Prediction:</h4>
                <p>Hen's Age: {hensage} weeks</p>
                <p>Egg Count: {prediction} eggs</p>
              </div>
            )}
          </ModalContainer>
        </ModalOverlay>
      )}
    </DashboardContainer>
  );
};

export default Dashboard;

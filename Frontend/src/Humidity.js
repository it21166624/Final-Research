import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import styled from "styled-components";
import { Line, Bar } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Tooltip,
  Legend,
} from "chart.js";
import jsPDF from "jspdf";
import "jspdf-autotable";

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Tooltip,
  Legend
);

// Styled Components
const Container = styled.div`
  max-width: 1200px;
  margin: 20px auto;
  padding: 0 20px;
  font-family: Arial, sans-serif;
`;

// (Other styled components omitted for brevity - keep them as in your original code)

const Grid = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: auto 400px;
  gap: 20px;

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto auto;
  }
`;

const TableContainer = styled.div`
  background: #f7f9fc;
  border-radius: 10px;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
  padding: 20px;
  grid-column: 1 / 3;

  @media (max-width: 768px) {
    grid-column: 1;
  }
`;

const SectionTitle = styled.h2`
  margin-bottom: 15px;
  color: #444;
`;

const Table = styled.table`
  width: 100%;
  border-collapse: collapse;
  color: #333;
`;

const Th = styled.th`
  background: #3498db;
  color: white;
  padding: 12px;
  text-transform: uppercase;
  border: 1px solid #ddd;
`;

const Td = styled.td`
  border: 1px solid #ddd;
  padding: 12px;
  text-align: center;
  background: white;
`;

const ChartContainer = styled.div`
  background: #f7f9fc;
  border-radius: 10px;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
  padding: 20px;
`;

export default function HumidityVentilationPage() {
  const [data, setData] = useState([]);
  const [chartData, setChartData] = useState(null);

  useEffect(() => {
    async function fetchData() {
      try {
        const response = await fetch("https://localhost:7039/api/sensordata/GetOldData");
        const apiData = await response.json();

        // Map API data to your required structure
        const mappedData = apiData.map((item) => {
          const dateObj = new Date(item.timestamp);
          const date = dateObj.toISOString().split("T")[0]; // YYYY-MM-DD
          const time = dateObj.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }); // HH:mm AM/PM

          return {
            date,
            time,
            humidity: item.humidity,
            ventilation: item.feed_Quantity,
          };
        });

        setData(mappedData);

        const times = mappedData.map((d) => d.time);
        const humidity = mappedData.map((d) => d.humidity);
        const ventilation = mappedData.map((d) => d.ventilation);

        setChartData({
          labels: times,
          datasets: [
            {
              label: "Humidity (%)",
              data: humidity,
              backgroundColor: "rgba(52, 152, 219, 0.5)",
              borderColor: "rgba(41, 128, 185, 1)",
              pointBackgroundColor: "rgba(41, 128, 185, 1)",
              borderWidth: 2,
              tension: 0.4,
              fill: false,
            },
            {
              label: "Ventilation Rate",
              data: ventilation,
              backgroundColor: "rgba(46, 204, 113, 0.5)",
              borderColor: "rgba(39, 174, 96, 1)",
              pointBackgroundColor: "rgba(39, 174, 96, 1)",
              borderWidth: 2,
              tension: 0.4,
              fill: false,
            },
          ],
        });
      } catch (error) {
        console.error("Error fetching data:", error);
      }
    }

    fetchData();
  }, []);

  const downloadPDF = () => {
    const doc = new jsPDF();
    doc.text("Humidity and Ventilation Report", 14, 10);
    doc.autoTable({
      startY: 20,
      head: [["Date", "Time", "Humidity (%)", "Ventilation Rate"]],
      body: data.map((row) => [row.date, row.time, row.humidity, row.ventilation]),
    });
    doc.save("humidity_ventilation_report.pdf");
  };

  const downloadCSV = () => {
    const csvContent = [
      ["Date", "Time", "Humidity (%)", "Ventilation Rate"],
      ...data.map((row) => [row.date, row.time, row.humidity, row.ventilation]),
    ]
      .map((e) => e.join(","))
      .join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.setAttribute("download", "humidity_ventilation_report.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <Container>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: "10px", marginBottom: "20px" }}>
        <button
          onClick={downloadPDF}
          style={{
            padding: "10px 20px",
            background: "#3498db",
            color: "#fff",
            border: "none",
            borderRadius: "5px",
          }}
        >
          Download PDF
        </button>
        <button
          onClick={downloadCSV}
          style={{
            padding: "10px 20px",
            background: "#2ecc71",
            color: "#fff",
            border: "none",
            borderRadius: "5px",
          }}
        >
          Download CSV
        </button>
      </div>

      <Grid>
        <TableContainer>
          <SectionTitle>Humidity and Ventilation Data</SectionTitle>
          <div style={{ overflowY: "auto", maxHeight: "250px" }}>
            <Table>
              <thead>
                <tr>
                  <Th>DATE</Th>
                  <Th>TIME</Th>
                  <Th>HUMIDITY (%)</Th>
                  <Th>VENTILATION RATE</Th>
                </tr>
              </thead>
              <tbody>
                {data.map((row, index) => (
                  <tr key={index}>
                    <Td>{row.date}</Td>
                    <Td>{row.time}</Td>
                    <Td>{row.humidity}</Td>
                    <Td>{row.ventilation}</Td>
                  </tr>
                ))}
              </tbody>
            </Table>
          </div>
        </TableContainer>

        <ChartContainer>
          <SectionTitle>Humidity & Ventilation Over Time</SectionTitle>
          {chartData && (
            <Line
              data={chartData}
              options={{
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                  y: { beginAtZero: true },
                },
              }}
              height={350}
            />
          )}
        </ChartContainer>

        <ChartContainer>
          <SectionTitle>Bar Chart Comparison</SectionTitle>
          {chartData && (
            <Bar
              data={chartData}
              options={{
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                  y: { beginAtZero: true },
                },
                plugins: {
                  legend: {
                    display: true,
                    position: "top",
                  },
                },
              }}
              height={350}
            />
          )}
        </ChartContainer>
      </Grid>
    </Container>
  );
}

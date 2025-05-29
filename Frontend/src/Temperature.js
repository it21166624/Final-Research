import React, { useState, useEffect } from "react";
import styled from "styled-components";
import { Line, Pie } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Tooltip,
  Legend,
  ArcElement,
} from "chart.js";
import jsPDF from "jspdf";
import "jspdf-autotable";

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Tooltip,
  Legend,
  ArcElement
);

const Container = styled.div`
  max-width: 1200px;
  margin: 20px auto;
  padding: 0 20px;
  font-family: Arial, sans-serif;
`;

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
  box-shadow: 0 0 15px rgba(0,0,0,0.1);
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
  box-shadow: 0 0 15px rgba(0,0,0,0.1);
  padding: 20px;
`;

export default function AnimalStressPage() {
  const [data, setData] = useState([]);
  const [chartData, setChartData] = useState(null);
  const [pieData, setPieData] = useState(null);

  useEffect(() => {
    async function fetchData() {
      try {
        const response = await fetch("https://localhost:7039/api/sensordata/GetOldData");
        const apiData = await response.json();

        const mappedData = apiData.map(item => {
          const dateObj = new Date(item.timestamp);
          const date = dateObj.toISOString().split("T")[0]; // YYYY-MM-DD
          const time = dateObj.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }); // HH:mm AM/PM

          return {
            date,
            time,
            Temp: item.temperature,
          };
        });

        setData(mappedData);

        const times = mappedData.map(d => d.time);
        const temps = mappedData.map(d => d.Temp);

        setChartData({
          labels: times,
          datasets: [
            {
              label: "Temperature (°C)",
              data: temps,
              borderColor: "#ff6384",
              backgroundColor: "rgba(255,99,132,0.2)",
              tension: 0.4,
              fill: true,
            },
          ],
        });

        const totalTemp = temps.reduce((acc, val) => acc + val, 0);

        setPieData({
          labels: ["Temperature"],
          datasets: [
            {
              data: [totalTemp],
              backgroundColor: ["#3b9c9c"],
              hoverOffset: 4,
            },
          ],
        });
      } catch (error) {
        console.error("Error fetching temperature data:", error);
      }
    }

    fetchData();
  }, []);

  const downloadPDF = () => {
    const doc = new jsPDF();
    doc.text("Temperature Measurement Report", 14, 10);
    doc.autoTable({
      startY: 20,
      head: [["Date", "Time", "Temperature (°C)"]],
      body: data.map(row => [row.date, row.time, row.Temp]),
    });
    doc.save("temperature_report.pdf");
  };

  const downloadCSV = () => {
    const csvContent = [
      ["Date", "Time", "Temperature (°C)"],
      ...data.map(row => [row.date, row.time, row.Temp]),
    ]
      .map(e => e.join(","))
      .join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.setAttribute("download", "temperature_report.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <Container>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: "10px", marginBottom: "20px" }}>
        <button onClick={downloadPDF} style={{ padding: "10px 20px", background: "#3498db", color: "#fff", border: "none", borderRadius: "5px" }}>
          Download PDF
        </button>
        <button onClick={downloadCSV} style={{ padding: "10px 20px", background: "#2ecc71", color: "#fff", border: "none", borderRadius: "5px" }}>
          Download CSV
        </button>
      </div>

      <Grid>
        <TableContainer>
          <SectionTitle>Temperature Measurement Data</SectionTitle>
          <div style={{ overflowY: "auto", maxHeight: "250px" }}>
            <Table>
              <thead>
                <tr>
                  <Th>Date</Th>
                  <Th>Time</Th>
                  <Th>Temperature (°C)</Th>
                </tr>
              </thead>
              <tbody>
                {data.map((row, i) => (
                  <tr key={i}>
                    <Td>{row.date}</Td>
                    <Td>{row.time}</Td>
                    <Td>{row.Temp}</Td>
                  </tr>
                ))}
              </tbody>
            </Table>
          </div>
        </TableContainer>

        <ChartContainer>
          <SectionTitle>Temperature Over Time</SectionTitle>
          {chartData && (
            <Line
              data={chartData}
              options={{ responsive: true, maintainAspectRatio: false }}
              height={350}
            />
          )}
        </ChartContainer>

        <ChartContainer>
          <SectionTitle>Totals Distribution</SectionTitle>
          {pieData && (
            <Pie
              data={pieData}
              options={{ responsive: true, maintainAspectRatio: false }}
              height={350}
            />
          )}
        </ChartContainer>
      </Grid>
    </Container>
  );
}

import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
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

// Styled components (keep your existing styles as is)
const Container = styled.div`
  max-width: 1200px;
  margin: 20px auto;
  padding: 0 20px;
  font-family: Arial, sans-serif;
`;

// ... other styled components (Header, Grid, TableContainer, etc.) keep as in your original code

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
  box-shadow: 0 0 15px rgba(180, 53, 53, 0.1);
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
  background: rgb(46, 159, 204);
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

export default function LighteningPage() {
  const [data, setData] = useState([]);
  const [chartData, setChartData] = useState(null);
  const [pieData, setPieData] = useState(null);

  useEffect(() => {
    async function fetchData() {
      try {
        const response = await fetch("https://localhost:7039/api/sensordata/GetOldData");
        const apiData = await response.json();

        // Map the API data:
        // brightness <- light_Hours
        // date/time from timestamp
        // usage - placeholder or keep 0 if no data from API

        const mappedData = apiData.map((item) => {
          const dateObj = new Date(item.timestamp);
          const date = dateObj.toISOString().split("T")[0]; // yyyy-mm-dd
          const time = dateObj.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });

          return {
            date,
            time,
            brightness: item.light_Hours,
            usage: 0, // or assign another API field if applicable
          };
        });

        setData(mappedData);

        const times = mappedData.map((d) => d.time);
        const brightness = mappedData.map((d) => d.brightness);
        const usage = mappedData.map((d) => d.usage);

        setChartData({
          labels: times,
          datasets: [
            {
              label: "Brightness Level",
              data: brightness,
              borderColor: "#e74c3c",
              backgroundColor: "rgba(230, 65, 65, 0.2)",
              tension: 0.4,
              fill: true,
            },
            {
              label: "Power Usage (kW)",
              data: usage,
              borderColor: "#3498db",
              backgroundColor: "rgba(42, 31, 192, 0.2)",
              tension: 0.4,
              fill: true,
            },
          ],
        });

        const totalBrightness = brightness.reduce((a, b) => a + b, 0);
        const totalUsage = usage.reduce((a, b) => a + b, 0);

        setPieData({
          labels: ["Total Brightness", "Total Power Usage"],
          datasets: [
            {
              data: [totalBrightness, totalUsage],
              backgroundColor: ["#e74c3c", "#3498db"],
              hoverOffset: 4,
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
    doc.text("Lightening Report", 14, 10);
    doc.autoTable({
      startY: 20,
      head: [["Date", "Time", "Brightness Level", "Power Usage (kW)"]],
      body: data.map((row) => [row.date, row.time, row.brightness, row.usage]),
    });
    doc.save("lightening_report.pdf");
  };

  const downloadCSV = () => {
    const csvContent = [
      ["Date", "Time", "Brightness Level", "Power Usage (kW)"],
      ...data.map((row) => [row.date, row.time, row.brightness, row.usage]),
    ]
      .map((e) => e.join(","))
      .join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.setAttribute("download", "lightening_report.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <Container>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: "10px", marginBottom: "20px" }}>
        <button
          onClick={downloadPDF}
          style={{ padding: "10px 20px", background: "#3498db", color: "#fff", border: "none", borderRadius: "5px" }}
        >
          Download PDF
        </button>
        <button
          onClick={downloadCSV}
          style={{ padding: "10px 20px", background: "#2ecc71", color: "#fff", border: "none", borderRadius: "5px" }}
        >
          Download CSV
        </button>
      </div>

      <Grid>
        <TableContainer>
          <SectionTitle>Lightening Data</SectionTitle>
          <div style={{ overflowY: "auto", maxHeight: "250px" }}>
            <Table>
              <thead>
                <tr>
                  <Th>DATE</Th>
                  <Th>TIME</Th>
                  <Th>Light Hours</Th>
                  {/* <Th>POWER USAGE (kW)</Th> */}
                </tr>
              </thead>
              <tbody>
                {data.map((row, index) => (
                  <tr key={index}>
                    <Td>{row.date}</Td>
                    <Td>{row.time}</Td>
                    <Td>{row.brightness}</Td>
                    {/* <Td>{row.usage}</Td> */}
                  </tr>
                ))}
              </tbody>
            </Table>
          </div>
        </TableContainer>

        <ChartContainer>
          <SectionTitle>Brightness & Power Usage Over Time</SectionTitle>
          {chartData && <Line data={chartData} options={{ responsive: true, maintainAspectRatio: false }} height={350} />}
        </ChartContainer>

        <ChartContainer>
          <SectionTitle>Totals Distribution</SectionTitle>
          {pieData && <Pie data={pieData} options={{ responsive: true, maintainAspectRatio: false }} height={350} />}
        </ChartContainer>
      </Grid>
    </Container>
  );
}

import React from "react";
import { BrowserRouter as Router, Routes, Route, useLocation } from "react-router-dom";
import Dashboard from "./Dashboard";
import Stress from "./Stress";
import Humidity from "./Humidity";
import Lightening from "./Lightening";
import Temperature from "./Temperature"
import Login from "./Login";
import Header from "./Header";
import './App.css';

function AppWrapper() {
  const location = useLocation();
  const hideHeaderRoutes = ["/", "/login"];
  const showHeader = !hideHeaderRoutes.includes(location.pathname.toLowerCase());

  return (
    <div className="App">
      {showHeader && <Header />}

      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/stress" element={<Stress />} />
        <Route path="/humidity" element={<Humidity />} />
        <Route path="/lightening" element={<Lightening />} />
        <Route path="/temperature" element={<Temperature />} />
      </Routes>
    </div>
  );
}

function App() {
  return (
    <Router>
      <AppWrapper />
    </Router>
  );
}

export default App;

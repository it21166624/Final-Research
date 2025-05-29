import React, { useState, useEffect } from "react";
import Dashboard from "./Dashboard";
import back from "./assets/bannerBg.jpg";
import mqtt from 'mqtt';
    
const SimpleStyledLogin = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [error, setError] = useState("");
  const client = mqtt.connect('ws://172.20.10.3:1883'); // <-- Updated connection string

  client.on('message', (topic, message) => {
    console.log(`Received message from ${topic}: ${message.toString()}`);
  });
  // Load Google Fonts dynamically
  useEffect(() => {
    const link = document.createElement("link");
    link.href = "https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap";
    link.rel = "stylesheet";
    document.head.appendChild(link);
  }, []);

  const handleSubmit = (e) => {
    e.preventDefault();
    // Hardcoded credentials
    const hardcodedEmail = "testfarmer@gmail.com";
    const hardcodedPassword = "FarmerTest123";

    if (email === hardcodedEmail && password === hardcodedPassword) {
      setIsLoggedIn(true);
      setError("");
    } else {
      setError("Invalid email or password.");
    }
  };

  if (isLoggedIn) {
    // Simple Dashboard screen
    return <Dashboard />;
  }

  return (
    <div style={styles.container}>
      <div style={styles.loginBox}>
        <h2 style={styles.title}>Welcome To Yeild PredictorX</h2>
        <p style={styles.subtitle}>Your Poultry Farm Assistant</p>
        <form onSubmit={handleSubmit} style={styles.form}>
          <input
            type="email"
            placeholder="Email Address"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            style={styles.input}
          />
          <input
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            style={styles.input}
          />
          {error && <p style={styles.errorText}>{error}</p>}
          <button type="submit" style={styles.button}>
            Log In
          </button>
        </form>
        <p style={styles.footerText}>
          Don't have an account?{" "}
          <a href="#" style={styles.link}>
            Sign up
          </a>
        </p>
      </div>
    </div>
  );
};

const styles = {
  container: {
    height: "100vh",
    backgroundImage: `url(${back})`,
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    fontFamily: "'Poppins', sans-serif",
  },
  loginBox: {
    backgroundColor: "#fff",
    padding: "40px 50px",
    borderRadius: 10,
    boxShadow: "0 10px 30px rgba(0,0,0,0.1)",
    width: 360,
    textAlign: "center",
  },
  title: {
    marginBottom: 30,
    fontWeight: 600,
    color: "#333",
    fontSize: 18,
  },
  subtitle: {
    marginTop: -20,
    marginBottom: 25,
    fontWeight: 500,
    fontSize: 14,
    fontStyle: "italic",
  },
  form: {
    display: "flex",
    flexDirection: "column",
  },
  input: {
    padding: "14px 20px",
    marginBottom: 20,
    borderRadius: 8,
    border: "1.5px solid #ddd",
    fontSize: 16,
    transition: "border-color 0.3s ease",
    outline: "none",
  },
  button: {
    padding: "14px 0",
    borderRadius: 8,
    border: "none",
    background:
      "linear-gradient(90deg, #6a11cb 0%, #2575fc 100%)",
    color: "#fff",
    fontWeight: "600",
    fontSize: 18,
    cursor: "pointer",
    transition: "background 0.3s ease",
  },
  footerText: {
    marginTop: 24,
    color: "#555",
    fontSize: 14,
  },
  link: {
    color: "#6a11cb",
    textDecoration: "none",
    fontWeight: "600",
  },
  errorText: {
    color: "red",
    marginBottom: 16,
    fontWeight: "600",
  },
  dashboardContainer: {
    height: "100vh",
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    alignItems: "center",
    fontFamily: "'Poppins', sans-serif",
    background: "#f5f5f5",
  },
  logoutButton: {
    marginTop: 30,
    padding: "12px 24px",
    fontSize: 16,
    borderRadius: 8,
    border: "none",
    background:
      "linear-gradient(90deg, #ff416c 0%, #ff4b2b 100%)",
    color: "#fff",
    cursor: "pointer",
    fontWeight: "600",
  },
};

export default SimpleStyledLogin;

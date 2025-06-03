require('dotenv').config();
const express = require('express');
const mysql = require('mysql2');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(express.json());
app.use(cors());

// Database connection
const db = mysql.createConnection({
    host: process.env.DB_HOST,
    user: process.env.DB_USER,
    password: process.env.DB_PASS,
    database: process.env.DB_NAME,
    port: process.env.DB_PORT || 3306
});

db.connect(err => {
    if (err) {
        console.error("Database connection failed:", err);
    } else {
        console.log("Connected to AWS MySQL database");
    }
});

// API Route: Add player data
app.post("/addPlayer", (req, res) => {
    const { username, score } = req.body;
    const query = "INSERT INTO players (username, score) VALUES (?, ?)";
    
    db.query(query, [username, score], (err, result) => {
        if (err) {
            res.status(500).json({ error: "Error inserting data", details: err });
        } else {
            res.json({ message: "Player data added successfully!" });
        }
    });
});

// API Route: Get all players
app.get("/players", (req, res) => {
    db.query("SELECT * FROM players", (err, results) => {
        if (err) {
            res.status(500).json({ error: "Error fetching data", details: err });
        } else {
            res.json(results);
        }
    });
});

app.listen(PORT, () => console.log(`Server running on port ${PORT}`));

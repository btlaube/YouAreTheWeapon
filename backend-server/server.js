require('dotenv').config();
const express = require('express');
const mysql = require('mysql2');
const cors = require('cors');

const app = express();
app.use(express.json()); // Enable JSON parsing
app.use(cors()); // Allow Unity to communicate with the server

// MySQL Database Connection
const db = mysql.createConnection({
    host: process.env.DB_HOST || 'localhost',
    user: process.env.DB_USER || 'root',
    password: process.env.DB_PASS || '',
    database: process.env.DB_NAME || 'game_data'
});

db.connect(err => {
    if (err) {
        console.error('Database connection failed:', err);
        return;
    }
    console.log('Connected to MySQL database');
});

// Test API Route
app.get('/', (req, res) => {
    res.send('Node.js backend is running!');
});

// API Route to Fetch Player Data
app.get('/player/:id', (req, res) => {
    const playerId = req.params.id;
    db.query('SELECT * FROM players WHERE id = ?', [playerId], (err, result) => {
        if (err) {
            res.status(500).send(err);
        } else {
            res.json(result);
        }
    });
});

// API Route to Save Player Data
app.post('/player', (req, res) => {
    const { id, name, score } = req.body;
    db.query('INSERT INTO players (id, name, score) VALUES (?, ?, ?)', [id, name, score], (err, result) => {
        if (err) {
            res.status(500).send(err);
        } else {
            res.json({ message: 'Player data saved', result });
        }
    });
});

// Start the Server
const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});

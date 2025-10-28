const express = require('express');
const path = require('path');
const fs = require('fs');
const app = express();

const port = process.env.PORT || 3000;

// Set EJS as view engine
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, '/'));

// Serve static files (CSS, JS, images)
app.use(express.static('public'));
// Parse form bodies
app.use(express.urlencoded({ extended: true }));

// home route
app.get('/', (req, res) => {
  res.render('frontside', { title: 'Login System' });
});

// handle login by reading user.json
app.post('/login', (req, res) => {
  const { username, password } = req.body || {};
  try {
    const jsonPath = path.join(__dirname, 'user.json');
    const raw = fs.readFileSync(jsonPath, 'utf8');
    const data = JSON.parse(raw);
    const list = Array.isArray(data.userdata) ? data.userdata : (data.userdata ? [data.userdata] : []);

    const found = list.find(u => u.username === username && u.password === password);
    if (found) {
      return res.send('Login succesfuldt');
    }
    return res.status(401).send('Forkert brugernavn eller adgangskode');
  } catch (err) {
    console.error('Fejl ved læsning af user.json:', err);
    return res.status(500).send('Server fejl');
  }
});

app.listen(port, () => {
  console.log(`Server is running on http://localhost:${port}`);
});
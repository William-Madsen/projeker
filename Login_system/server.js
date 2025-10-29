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

// signup route
app.get('/signup', (req, res) => {
  res.render('signup', { title: 'Sign Up' });
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
    return res.status(401).render('frontside', { title: 'Login System', alert: 'Ugyldigt brugernavn eller adgangskode' });
  } catch (err) {
    console.error('Fejl ved læsning af user.json:', err);
    return res.status(500).send('Server fejl');
  }
});

// signup handler
app.post('/signup', (req, res) => {
  const { username, password, confirmPassword } = req.body || {};
  try {
    if (!username || !password || !confirmPassword) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Udfyld alle felter' });
    }
    if (password !== confirmPassword) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Adgangskode og Gentag adgangskode skal være ens' });
    }

    const jsonPath = path.join(__dirname, 'user.json');
    let data = { userdata: [] };
    if (fs.existsSync(jsonPath)) {
      const raw = fs.readFileSync(jsonPath, 'utf8');
      data = JSON.parse(raw);
    }
    if (!Array.isArray(data.userdata)) {
      data.userdata = data.userdata ? [data.userdata] : [];
    }

    const exists = data.userdata.some(u => u.username === username);
    if (exists) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Brugernavnet er allerede i brug' });
    }

    data.userdata.push({ username, password });
    fs.writeFileSync(jsonPath, JSON.stringify(data, null, 2), 'utf8');

    return res.status(201).render('frontside', { title: 'Login System', alert: 'Bruger oprettet. Log ind.' });
  } catch (err) {
    console.error('Fejl ved skrivning til user.json:', err);
    return res.status(500).render('signup', { title: 'Sign Up', alert: 'Server fejl' });
  }
});

app.listen(port, () => {
  console.log(`Server is running on http://localhost:${port}`);
});
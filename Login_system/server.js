// Importer afhængigheder (webserver, stier, filsystem, sessioner og password-hash)
const express = require('express');
const path = require('path');
const fs = require('fs');
const session = require('express-session');
const bcrypt = require('bcryptjs');

// Opret Express app
const app = express();

// Port til serveren (brug 3000 som standard)
const port = 3000;

// Konfigurer EJS på mappe med .ejs-filer
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, '/'));

// Konfigurer sessioner (gemmer et lille login-token i en cookie)
app.use(
  session({
    secret: 'dev-secret', // hemmelig nøgle til signering af session-cookies
    resave: false,        // gem ikke session hvis den ikke er ændret
    saveUninitialized: false, // opret ikke tomme sessioner
    cookie: { maxAge: 1000 * 60 * 60 }, // session varer 1 time
  })
);

// Forside (login formular)
app.get('/', (req, res) => {
  res.render('frontside', { title: 'Login System' });
});

// Vis signup-side
app.get('/signup', (req, res) => {
  res.render('signup', { title: 'Sign Up' });
});

// Beskyttet side: kun synlig hvis man er logget ind (ellers redirect til forside)
app.get('/login', (req, res) => {
  if (!req.session.user) return res.redirect('/');
  res.render('login', { title: 'Logget ind', username: req.session.user.username });
});

// Håndter login: læs brugere fra user.json og sammenlign med inpute
app.post('/login', (req, res) => {
  const { username, password } = req.body || {};
  try {
    // Find og læs brugerdatabase (user.json)
    const jsonPath = path.join(__dirname, 'user', 'user.json');
    const raw = fs.readFileSync(jsonPath, 'utf8');
    const data = JSON.parse(raw);
    // Sørg for at have en liste af brugere
    const list = Array.isArray(data.userdata) ? data.userdata : (data.userdata ? [data.userdata] : []);

    // Find bruger med matching brugernavn
    const found = list.find(u => u.username === username);
    // Sammenlign indtastet kodeord med lagret hash
    if (found && bcrypt.compareSync(password, found.password)) {
      // Gem "logged-in" status i sessionen
      req.session.user = { username: found.username };
      return res.redirect('/login'); // vis den beskyttede side
    }
    // Forkert login -> vis fejl på forsiden
    return res.status(401).render('frontside', { title: 'Login System', alert: 'Ugyldigt brugernavn eller adgangskode' });
  } catch (err) {
    // Hvis der er fejl ved læsning af siden, log og send 500
    console.error('Fejl ved læsning af user.json:', err);
    return res.status(500).send('Server fejl');
  }
});

// Log ud: ryd session og send tilbage til forsiden
app.post('/logout', (req, res) => {
  req.session.destroy(() => {
    res.redirect('/');
  });
});

// Håndter signup: valider felter, tjek dublet, hash kodeord og gem i user.json
app.post('/signup', (req, res) => {
  const { username, password, confirmPassword } = req.body || {};
  try {
    // Simpel validering af input
    if (!username || !password || !confirmPassword) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Udfyld alle felter' });
    }
    if (password !== confirmPassword) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Adgangskode og Gentag adgangskode skal være ens' });
    }

    // Indlæs eksisterende brugere (eller opret tom liste)
    const jsonPath = path.join(__dirname, 'user', 'user.json');
    let data = { userdata: [] };
    if (fs.existsSync(jsonPath)) {
      const raw = fs.readFileSync(jsonPath, 'utf8');
      data = JSON.parse(raw);
    }
    if (!Array.isArray(data.userdata)) {
      data.userdata = data.userdata ? [data.userdata] : [];
    }

    // Tjek om brugernavn allerede findes
    const exists = data.userdata.some(u => u.username === username);
    if (exists) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Brugernavnet er allerede i brug' });
    }

    // Hash kodeord og gem ny bruger
    const hashed = bcrypt.hashSync(password, 10);
    data.userdata.push({ username, password: hashed });
    fs.writeFileSync(jsonPath, JSON.stringify(data, null, 2), 'utf8');

    // Succes -> tilbage til login med besked
    return res.status(201).render('frontside', { title: 'Login System', alert: 'Bruger oprettet. Log ind.' });
  } catch (err) {
    // Fejl ved skrivning/parsing -> vis generel fejl
    console.error('Fejl ved skrivning til user.json:', err);
    return res.status(500).render('signup', { title: 'Sign Up', alert: 'Server fejl' });
  }
});

// Start HTTP-serveren
app.listen(port, () => {
  console.log(`Server is running on http://localhost:${port}`);
});
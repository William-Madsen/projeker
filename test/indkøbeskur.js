const Items = ["Æg", "Mælk", "Brød", "Smør", "Ost"];
let Cart = [];

function AddToCart(item) {
  if (Items.includes(item)) {
    Cart.push(item);
  } else {
    console.log("Den vare findes ikke på listen!");
  }
}

const readline = require("readline");
const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
});

function askUser(){
  console.log("Her er de vare man kan vælge:", Items);
  rl.question("Hvad vil du købe? (skriv 'exit' for at afslutte) ", (svar) => {
    const cleaned = svar.trim().toLowerCase();

    if (cleaned === "exit" || cleaned === "quit") {
      console.log("Afslutter. Din kurv:", Cart);
      rl.close();
      return;
    }

    const match = Items.find(x => x.toLowerCase() === cleaned);

    if (match) {
      AddToCart(match);
      console.log("Tilføjet:", match);
    } else {
      console.log("Den vare findes ikke på listen! Prøv igen.");
    }


    askUser();
  });
}
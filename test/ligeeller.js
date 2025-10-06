const readline = require('readline');

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

rl.question('Indtast et tal: ', (input) => {
    const number = parseInt(input, 10);

    if (isNaN(number)) {
        console.log('Det er ikke et gyldigt tal.');
    } else if (number % 2 === 0) {
        console.log('Tallet er lige.');
    } else {
        console.log('Tallet er ulige.');
    }

    rl.close();
});
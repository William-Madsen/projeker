Console.WriteLine("dette er en lommeregner :)");
Console.WriteLine("Skriv det første tal:");
string input1 = Console.ReadLine(); // Læs det første tal
Console.WriteLine("Skriv det andet tal:");
string input2 = Console.ReadLine(); // Læs det andet tal
double num1 = 0;  // initialiserer variablerne
double num2 = 0; // initialiserer variablerne
if (!double.TryParse(input1, out num1) || !double.TryParse(input2, out num2))
{
    Console.WriteLine("Error: Ugyldigt talinddata.");
    return;
    // hvis der er et bogstav eller andet end et tal
    // så stopper programmet og skriver en fejlbesked
}
Console.WriteLine("Vælg en operation (+, -, *, /, kvrod, ^):");
string operation = Console.ReadLine();
double result = 0;

switch (operation)
{
    // regner de forskellige operationer sammen
    case "+":
        result = num1 + num2;
        // pulser de to tal sammen
        break;
    case "-":
        result = num1 - num2;
        // minus de to tal sammen
        break;
    case "*":
        result = num1 * num2;
        // gange de to tal sammen
        break;
    case "/":
        if (num2 != 0)
        // hvis det andet tal ikke er 0 så køre den koden nedenunder
        {
            result = num1 / num2;
            // divider de to tal sammen
        }
        else
        // hvis det andet tal er 0 så køre den koden nedenunder
        {
            Console.WriteLine("Error: Division med nul er ikke tilladt.");
            return;
            // hvis du divider med 0 så stopper programmet og skriver en fejlbesked
        }
        break;
    case "kvrod":
        if (num1 >= 0 && num2 >= 0)
        // hvis begge tal er større end eller lig med 0 så køre den koden nedenunder
        {
            result = Math.Sqrt(num1) + Math.Sqrt(num2);
            // regner kvadratroden af de to tal sammen
        }
        else
        // hvis et af tallene er mindre end 0 så køre den koden nedenunder
        {
            Console.WriteLine("Error: Kvadratrod af negative tal er ikke defineret i de reelle tal.");
            return;
            // hvis du prøver at regne kvadratroden af et negativt tal så stopper programmet og skriver en fejlbesked
        }
        break;
    case "^":
        result = Math.Pow(num1, num2);
        // regner det første tal opløftet i det andet tal
        break;

    default:
        Console.WriteLine("Error: Ugyldig operation.");
        return;
    // hvis du skriver en anden operation end de 4 så stopper programmet og skriver en fejlbesked
}
Console.WriteLine($"Resultatet er: {result}");
// udskriver resultatet
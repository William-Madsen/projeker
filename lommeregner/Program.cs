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
Console.WriteLine("Vælg en operation (+, -, *, /):");
string operation = Console.ReadLine();
double result = 0;

switch (operation)
{
    // regner de forskellige operationer sammen
    case "+":
        result = num1 + num2;
        break;
    case "-":
        result = num1 - num2;
        break;
    case "*":
        result = num1 * num2;
        break;
    case "/":
        if (num2 != 0)
        {
            result = num1 / num2;
        }
        else
        {
            Console.WriteLine("Error: Division med nul er ikke tilladt.");
            return;
            // hvis du divider med 0 så stopper programmet og skriver en fejlbesked
        }
        break;
    default:
        Console.WriteLine("Error: Ugyldig operation.");
        return;
    // hvis du skriver en anden operation end de 4 så stopper programmet og skriver en fejlbesked
}
Console.WriteLine($"Resultatet er: {result}");
// udskriver resultatet

Console.WriteLine("dette er en lommeregner :)");
Console.WriteLine("Skriv det første tal:");
string input1 = Console.ReadLine();
Console.WriteLine("Skriv det andet tal:");
string input2 = Console.ReadLine();
double num1 = 0;
double num2 = 0;
if (!double.TryParse(input1, out num1) || !double.TryParse(input2, out num2))
{
    Console.WriteLine("Error: Ugyldigt talinddata.");
    return;
}
Console.WriteLine("Vælg en operation (+, -, *, /):");
string operation = Console.ReadLine();
double result = 0;

switch (operation)
{
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
        }
        break;
    default:
        Console.WriteLine("Error: Ugyldig operation.");
        return;
}
Console.WriteLine($"Resultatet er: {result}");
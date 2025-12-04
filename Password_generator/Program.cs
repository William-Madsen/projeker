using System;
using System.Text;

class Program
{

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Velkommen til Password Generator og Tester!");
            Console.WriteLine("1. Generer og test et password");
            Console.WriteLine("2. Test password styrke");
            Console.WriteLine("3. Afslut");
            Console.Write("Vælg en mulighed (1, 2 eller 3): ");
            string choice = Console.ReadLine() ?? "";

            if (choice == "1")
            {
    
        // Get password length from user
        int length;
        while (true)
        {
            Console.Write("Hvor lang skal Passwordet været: ");
            string input = Console.ReadLine() ?? "";
            
            if (int.TryParse(input, out length))
            {
                if (length > 0)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Passwordet skal være over 0!");
                }
            }
            else
            {
                Console.WriteLine("Indtast venligst kun tal!");
            }
        }

        // Generate password
        string password = GeneratePassword(length);
        Console.WriteLine($"Generated Password: {password}");
        
        // Test password strength
        int strength = CheckedPassword.PasswordTester.PasswordStrength(password);
        Console.WriteLine($"Password strength score: {strength}/7");
        
        // Interpret score
        if (strength <= 2) Console.WriteLine("Svagt password");
        else if (strength <= 4) Console.WriteLine("Middel password");
        else if (strength <= 6) Console.WriteLine("Stærkt password");
        else Console.WriteLine("Meget stærkt password");

            }
            else if (choice == "2")
            {
                Console.Write("Indtast et password til test: ");
                string password = Console.ReadLine() ?? "";

                // Test password strength
                int strength = CheckedPassword.PasswordTester.PasswordStrength(password);
                Console.WriteLine($"Password strength score: {strength}/7");

                // Interpret score
                if (strength <= 2) Console.WriteLine("Svagt password");
                else if (strength <= 4) Console.WriteLine("Middel password");
                else if (strength <= 6) Console.WriteLine("Stærkt password");
                else Console.WriteLine("Meget stærkt password");
            }
            else if (choice == "3")
            {
                Console.WriteLine("Afslutter programmet. Farvel!");
                break;
            }
            else
            {
                Console.WriteLine("Ugyldigt valg. Prøv igen.");
            }

            Console.WriteLine();
        }
    }


    static string GeneratePassword(int length)
    {
        string lowercase = "abcdefghijklmnopqrstuvwxyz";
        string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string digits = "0123456789";
        string specialChars = "!#$&-_?";
        string allChars = lowercase + uppercase + digits + specialChars;

        StringBuilder password = new StringBuilder();
        Random random = new Random();

        for (int i = 0; i < length; i++)
        {
            int index = random.Next(allChars.Length);
            password.Append(allChars[index]);
        }

        return password.ToString();
    }
}

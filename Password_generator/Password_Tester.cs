using System;
using System.Text.RegularExpressions;

namespace CheckedPassword
{
    class PasswordTester
    {
            
            public static void TestPassword()
            {
                string lowercase = "abcdefghijklmnopqrstuvwxyzåæø";
                string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÆØ";
                string digits = "0123456789";
                string specialChars = "!#$&-_?";
                string passinput = lowercase + uppercase + digits + specialChars;
                Console.WriteLine(PasswordStrength(passinput));
            }

            public static int PasswordStrength(string password)
            {
                int score = 0;
                int length = password?.Length ?? 0;

                if (length >= 8 && length <= 10) score += 1;
                else if (length >= 11 && length <= 15) score += 2;
                else if (length >= 16) score += 3;

                string s = password ?? string.Empty;
                if (Regex.IsMatch(s, "[a-z]")) score++;
                if (Regex.IsMatch(s, "[A-Z]")) score++;
                if (Regex.IsMatch(s, "[0-9]")) score++;
                if (Regex.IsMatch(s, "[^a-zA-Z0-9]")) score++;

                return score;
            }
    }
}
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: no arguments provided.");
            return;
        }

        string input = string.Join("", args);

        // Validate characters
        foreach (char c in input)
        {
            if (!((c >= '0' && c <= '9') || c == ';' || c == '-'))
            {
                Console.WriteLine("Error: only digits (0-9), ';' and '-' are allowed.");
                return;
            }

        }
        Console.WriteLine(input);

        
    }
}

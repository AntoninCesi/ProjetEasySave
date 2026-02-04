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
        if (args[0].Length == 3)
        {
            Console.WriteLine(args[0].Length);
            char first = args[0][0];
            char middle = args[0][1];
            char last = args[0][2];

            if (char.IsDigit(first) && char.IsDigit(last) && (middle == '-' || middle == ','))
            {
                Console.WriteLine("Valid input: " + args[0]);
            }
            else
            {
                Console.WriteLine("Error: input must be in the form '3-2' or '4,9'.");
            }
        }
        else
        {
            Console.WriteLine("Error: input must be exactly 3 characters long.");
        }
    }
        

        


    
}

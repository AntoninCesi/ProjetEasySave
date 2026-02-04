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
        Console.WriteLine(args[0][0]);

        
    }
}

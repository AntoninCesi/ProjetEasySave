using System;
using EasySave.ViewModels;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Controller controller = new Controller(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
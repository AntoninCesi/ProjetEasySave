using System;

class Program
{
    static void Main(string[] args)
    {
        BackupController controller = new BackupController();
        //ConsoleUI ui = new ConsoleUI();

        if (args.Length > 0)
        {
            Console.WriteLine("yep");
        }
        else
        {
            Console.WriteLine("nan");
        }
    }

}

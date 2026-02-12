using System;
using System.Threading.Tasks;
using EasyLog;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var logger = new EasyLogger();

            Parallel.For(0, 100, i =>
            {
                logger.Write(LogEvent.FileCopied(
                    "JOB_PARALLEL",
                    $"src_{i}.txt",
                    $"dst_{i}.txt",
                    i * 100,
                    i
                ));
            });
            Console.WriteLine("100 logs écrits en parallèle.");
            Console.ReadKey();
        }
    }
}

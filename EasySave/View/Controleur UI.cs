using System;

class ConsoleUI
{
    /*static void Main()
    {
        MenuPrincipal();
    }*/

    static void MenuPrincipal()
    {
        string langue = ChoisirLangue();
        bool quitter = false;

        while (!quitter)
        {
            Console.Clear();

            if (langue == "FR")
            {
                Console.WriteLine("===== MENU =====");
                Console.WriteLine("1 - Sauvegarde complète");
                Console.WriteLine("2 - Sauvegarde incrémentale");
                Console.WriteLine("3 - Charger");
                Console.WriteLine("4 - Quitter");
                Console.Write("Votre choix : ");
            }
            else
            {
                Console.WriteLine("===== MENU =====");
                Console.WriteLine("1 - Full Save");
                Console.WriteLine("2 - Incremental Save");
                Console.WriteLine("3 - Load");
                Console.WriteLine("4 - Exit");
                Console.Write("Your choice: ");
            }

            string? choix = Console.ReadLine()?.Trim();

            switch (choix)
            {
                case "1":
                    Console.WriteLine(langue == "FR" ? "Sauvegarde complète lancée..." : "Full save started...");
                    break;
                case "2":
                    Console.WriteLine(langue == "FR" ? "Sauvegarde incrémentale lancée..." : "Incremental save started...");
                    break;
                case "3":
                    Console.WriteLine(langue == "FR" ? "Chargement en cours..." : "Loading...");
                    break;
                case "4":
                    quitter = true;
                    Console.WriteLine(langue == "FR" ? "Au revoir." : "Goodbye.");
                    break;
                default:
                    Console.WriteLine(langue == "FR" ? "Choix invalide. Appuyez sur une touche pour réessayer." : "Invalid choice. Press any key to retry.");
                    break;
            }

            if (!quitter)
            {
                Console.WriteLine();
                Console.WriteLine(langue == "FR" ? "Appuyez sur une touche pour revenir au menu." : "Press any key to return to the menu.");
                Console.ReadKey(true);
            }
        }
    }

    private static string ChoisirLangue()
    {
        while (true)
        {
            Console.Clear();
            Console.Write("Choisir la langue / Choose language (FR/EN) : ");
            string? raw = Console.ReadLine()?.Trim().ToUpperInvariant();

            if (raw == "FR" || raw == "EN")
                return raw;

            Console.WriteLine("Entrée invalide. Veuillez saisir 'FR' ou 'EN'.");
            Console.WriteLine("Press any key to retry / Appuyez sur une touche pour réessayer.");
            Console.ReadKey(true);
        }
    }
}
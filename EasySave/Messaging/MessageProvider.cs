using System;

namespace EasySave.Messaging
{
    public class MessageProvider
    {
        private readonly bool isFrench;

        public MessageProvider(bool isFrench)
        {
            this.isFrench = isFrench;
        }

        // Resolves a message to its string representation in the correct language
        public string Resolve(Message message)
        {
            return message.Type switch
            {
                // Menu
                MessageType.MenuTitle =>
                    isFrench ? "===== MENU =====" : "===== MENU =====",

                MessageType.MenuOption1 =>
                    isFrench ? "1 - Sauvegarde complète" : "1 - Full Save",

                MessageType.MenuOption2 =>
                    isFrench ? "2 - Sauvegarde incrémentale" : "2 - Incremental Save",

                MessageType.MenuOption3 =>
                    isFrench ? "3 - Charger" : "3 - Load",

                MessageType.MenuOption4 =>
                    isFrench ? "4 - Quitter" : "4 - Exit",

                MessageType.MenuPrompt =>
                    isFrench ? "Votre choix : " : "Your choice: ",

                MessageType.BackToMenu =>
                    isFrench ? "Appuyez sur une touche pour revenir au menu." : "Press any key to return to the menu.",

                // Job actions
                MessageType.JobStarted =>
                    message.Parameters.Length > 0
                        ? (isFrench ? $"Job {message.Parameters[0]} démarré." : $"Job {message.Parameters[0]} started.")
                        : (isFrench ? "Job démarré." : "Job started."),

                MessageType.Loading =>
                    isFrench ? "Chargement en cours..." : "Loading...",

                MessageType.Goodbye =>
                    isFrench ? "Au revoir." : "Goodbye.",

                MessageType.InvalidChoice =>
                    isFrench ? "Choix invalide. Appuyez sur une touche pour réessayer." : "Invalid choice. Press any key to retry.",

                // Errors
                MessageType.InvalidFormat =>
                    isFrench ? "Erreur : le format doit être '3-5' ou '3,5'." : "Error: input must be in the form '3-5' or '3,5'.",

                MessageType.JobNotFound =>
                    isFrench ? "Erreur : job de sauvegarde introuvable." : "Error: backup job not found.",

                MessageType.JobNotFoundWithId =>
                    message.Parameters.Length > 0
                        ? (isFrench ? $"Erreur : le job {message.Parameters[0]} n'existe pas." : $"Error: job {message.Parameters[0]} does not exist.")
                        : (isFrench ? "Erreur : job introuvable." : "Error: job not found."),

                MessageType.JobState =>
                    message.Parameters.Length >= 2
                        ? (isFrench ? $"État du job {message.Parameters[0]} : {message.Parameters[1]}" : $"Job {message.Parameters[0]} state: {message.Parameters[1]}")
                        : string.Empty,

                MessageType.Error =>
                    message.Parameters.Length > 0
                        ? (isFrench ? $"Erreur : {message.Parameters[0]}" : $"Error: {message.Parameters[0]}")
                        : (isFrench ? "Erreur inconnue." : "Unknown error."),

                _ => string.Empty
            };
        }
    }
}

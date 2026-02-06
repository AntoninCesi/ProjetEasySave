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

        public string Resolve(Message message)
        {
            return message.Type switch
            {
                // Menu
                MessageType.MenuTitle =>
                    "===== MENU =====",

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
                    isFrench
                        ? "Appuyez sur une touche pour revenir au menu."
                        : "Press any key to return to the menu.",

                // Directories
                MessageType.AskSourceDirectory =>
                    isFrench
                        ? "Veuillez entrer le dossier source :"
                        : "Please enter the source directory:",

                MessageType.AskDestinationDirectory =>
                    isFrench
                        ? "Veuillez entrer le dossier de destination :"
                        : "Please enter the destination directory:",

                MessageType.InvalidDirectory =>
                    isFrench
                        ? "Dossier invalide. Veuillez réessayer."
                        : "Invalid directory. Please try again.",

                // Job actions
                MessageType.JobStarted =>
                    isFrench
                        ? "Sauvegarde démarrée."
                        : "Backup started.",
                MessageType.JobName =>
                isFrench
                ? "Nom du travail."
                : "Job name.",

                MessageType.Loading =>
                    isFrench ? "Chargement en cours..." : "Loading...",

                MessageType.Goodbye =>
                    isFrench ? "Au revoir." : "Goodbye.",

                MessageType.InvalidChoice =>
                    isFrench
                        ? "Choix invalide."
                        : "Invalid choice.",

                // Errors
                MessageType.InvalidFormat =>
                    isFrench
                        ? "Erreur : format invalide."
                        : "Error: invalid format.",

                MessageType.JobNotFound =>
                    isFrench
                        ? "Job introuvable."
                        : "Job not found.",

                MessageType.JobNotFoundWithId =>
                    isFrench
                        ? $"Le job {message.Parameters[0]} n'existe pas."
                        : $"Job {message.Parameters[0]} does not exist.",

                MessageType.JobState =>
                    isFrench
                        ? $"État du job : {message.Parameters[0]}"
                        : $"Job state: {message.Parameters[0]}",

                MessageType.Error =>
                    isFrench
                        ? $"Erreur : {message.Parameters[0]}"
                        : $"Error: {message.Parameters[0]}",

                _ => string.Empty
            };
        }
    }
}

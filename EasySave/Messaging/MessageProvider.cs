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
                MessageType.InvalidFormat =>
                    isFrench
                        ? "Erreur : le format doit être '3-5' ou '3;5'."
                        : "Error: format must be '3-5' or '3;5'.",

                MessageType.JobNotFound =>
                    isFrench
                        ? "Erreur : travail de sauvegarde introuvable."
                        : "Error: backup job not found.",

                MessageType.JobNotFoundWithId =>
                    isFrench
                        ? $"Erreur : le travail {message.Parameters[0]} n'existe pas."
                        : $"Error: job {message.Parameters[0]} does not exist.",

                MessageType.JobStarted =>
                    isFrench
                        ? $"Travail {message.Parameters[0]} démarré."
                        : $"Job {message.Parameters[0]} started.",

                MessageType.JobState =>
                    isFrench
                        ? $"État du travail {message.Parameters[0]} : {message.Parameters[1]}"
                        : $"Job {message.Parameters[0]} state: {message.Parameters[1]}",

                MessageType.Loading =>
                    isFrench
                        ? "Chargement en cours..."
                        : "Loading...",

                MessageType.Goodbye =>
                    isFrench
                        ? "Au revoir."
                        : "Goodbye.",

                MessageType.InvalidChoice =>
                    isFrench
                        ? "Choix invalide. Appuyez sur une touche pour réessayer."
                        : "Invalid choice. Press any key to retry.",

                MessageType.Error =>
                    isFrench
                        ? $"Erreur : {message.Parameters[0]}"
                        : $"Error: {message.Parameters[0]}",

                MessageType.BackToMenu =>
                    isFrench
                        ? "Appuyez sur une touche pour revenir au menu."
                        : "Press any key to return to menu.",

                MessageType.MaxJobsReached =>
                    isFrench
                        ? $"Limite de {message.Parameters[0]} travaux atteinte."
                        : $"Maximum of {message.Parameters[0]} jobs reached.",

                MessageType.JobAlreadyExists =>
                    isFrench
                        ? $"Un travail nommé '{message.Parameters[0]}' existe déjà."
                        : $"A job named '{message.Parameters[0]}' already exists.",

                MessageType.NoJobsAvailable =>
                    isFrench
                        ? "Aucun travail disponible."
                        : "No jobs available.",

                _ => string.Empty
            };
        }
    }
}
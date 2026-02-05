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
                        ? "Erreur : le format doit être '3-5' ou '3,5'."
                        : "Error: input must be in the form '3-5' or '3,5'.",

                MessageType.JobNotFound =>
                    isFrench
                        ? "Erreur : job de sauvegarde introuvable."
                        : "Error: backup job not found.",

                MessageType.JobNotFoundWithId =>
                    isFrench
                        ? $"Erreur : le job {message.Parameters[0]} n'existe pas."
                        : $"Error: job {message.Parameters[0]} does not exist.",

                MessageType.JobStarted =>
                    isFrench
                        ? $"Job {message.Parameters[0]} démarré."
                        : $"Job {message.Parameters[0]} started.",

                MessageType.JobState =>
                    isFrench
                        ? $"État du job {message.Parameters[0]} : {message.Parameters[1]}"
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
                        : "Press any key to return to the menu.",

                _ => string.Empty
            };
        }
    }
}

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
                MessageType.MenuTitle => "===== MENU =====",
                MessageType.MenuOption1 => isFrench ? "1 - Sauvegarde complète" : "1 - Full Save",
                MessageType.MenuOption2 => isFrench ? "2 - Sauvegarde incrémentale" : "2 - Incremental Save",
                MessageType.MenuOption3 => isFrench ? "3 - Charger" : "3 - Load",
                MessageType.MenuOption4 => isFrench ? "4 - Quitter" : "4 - Exit",
                MessageType.MenuPrompt => isFrench ? "Votre choix : " : "Your choice: ",
                MessageType.BackToMenu => isFrench ? "Appuyez sur une touche pour revenir au menu." : "Press any key to return to the menu.",

                MessageType.AskSourceDirectory => isFrench ? "Veuillez entrer le dossier source :" : "Please enter the source directory:",
                MessageType.AskDestinationDirectory => isFrench ? "Veuillez entrer le dossier de destination :" : "Please enter the destination directory:",
                MessageType.JobName => isFrench ? "Nom du travail :" : "Job name:",

                // Validation errors
                MessageType.SourceAndDestIdentical => isFrench ? "Erreur : La source et la destination sont identiques." : "Error: Source and destination are identical.",
                MessageType.SourceDoesNotExist => isFrench ? "Erreur : Le dossier source n'existe pas." : "Error: Source directory does not exist.",
                MessageType.InsufficientDiskSpace => isFrench ? "Erreur : Espace disque insuffisant sur la destination." : "Error: Not enough disk space on destination drive.",
                MessageType.FileLocked => isFrench ? "Erreur : Un fichier est utilisé par un autre processus." : "Error: A file is being used by another process.",
                MessageType.EmptyJobName => isFrench ? "Erreur : Le nom du travail ne peut pas être vide." : "Error: Job name cannot be empty.",
                MessageType.DestinationAccessDenied => isFrench ? "Erreur : Destination inaccessible ou protégée." : "Error: Destination path is not accessible or write-protected.",
                MessageType.SourceEmpty => isFrench ? "Attention : Le dossier source est vide." : "Warning: Source directory is empty. Nothing to backup.",
                MessageType.DuplicateJobName => isFrench ? "Erreur : Un travail avec ce nom existe déjà." : "Error: A backup job with this name already exists.",
                MessageType.NoJobAvailable => isFrench ? "Aucun job disponible." : "No available job",
                MessageType.SelectJob => isFrench ? "Quel job voulez-vous lancer ?" : "What kind of job do you want to start?",
                MessageType.BackupStarted => isFrench ? "Backup lancé." : "Backup initiated.",


                                _ => string.Empty
            };
        }
    }
}
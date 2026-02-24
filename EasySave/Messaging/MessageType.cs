namespace EasySave.Messaging
{
    public enum MessageType
    {
        // Menu
        MenuTitle,
        MenuOption1,
        MenuOption2,
        MenuOption3,
        MenuOption4,
        MenuPrompt,
        BackToMenu,

        // Actions & Feedback
        JobStarted,
        Loading,
        Goodbye,
        InvalidChoice,
        JobName,
        NoJobAvailable,
        SelectJob,
        BackupStarted,
        EncryptionSuccess,    
        BackupBlockedMessage,
        AskDestinationDirectory,
        AskSourceDirectory,
        MenuOption5,
        BackupEnd,

        // Error & good
        InvalidFormat,
        JobNotFound,
        SourceAndDestIdentical,
        SourceDoesNotExist,
        InsufficientDiskSpace,
        FileLocked,
        EmptyJobName,
        DestinationAccessDenied,
        SourceEmpty,
        DuplicateJobName
    }
}
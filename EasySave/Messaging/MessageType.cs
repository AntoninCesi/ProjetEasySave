using System;

namespace EasySave.Messaging
{
    public enum MessageType
    {
        MenuTitle,
        MenuOption1,
        MenuOption2,
        MenuOption3,
        MenuOption4,
        MenuPrompt,
        BackToMenu,

        // Path selection
        AskSourceDirectory,
        AskDestinationDirectory,
        InvalidDirectory,

        // Job actions
        JobStarted,
        Loading,
        Goodbye,
        InvalidChoice,
        JobName,
        NoJobAvailable,
        SelectJob,
        BackupStarted,


        // Errors
        InvalidFormat,
        JobNotFound,
        JobNotFoundWithId,
        JobState,
        Error,

        // Security & Validation errors
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
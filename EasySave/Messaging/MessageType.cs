using System;


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

        // Job actions
        JobStarted,
        Loading,
        Goodbye,
        InvalidChoice,

        // Errors
        InvalidFormat,
        JobNotFound,
        JobNotFoundWithId,
        JobState,
        Error
    }
}


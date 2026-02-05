using System;


namespace EasySave.Messaging
{
    public enum MessageType
    {
        InvalidFormat,
        JobNotFound,
        JobNotFoundWithId,
        JobStarted,
        Loading,
        Goodbye,
        InvalidChoice,
        JobState,
        Error,
        BackToMenu
    }
}


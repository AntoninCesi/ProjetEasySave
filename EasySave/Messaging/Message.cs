namespace EasySave.Messaging
{
    public class Message
    {
        public MessageType Type { get; }
        public object[] Parameters { get; }

        public Message(MessageType type, params object[] parameters)
        {
            Type = type;
            Parameters = parameters ?? System.Array.Empty<object>();
        }
    }
}

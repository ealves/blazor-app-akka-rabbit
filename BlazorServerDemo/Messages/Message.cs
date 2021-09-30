namespace BlazorServerDemo.Messages
{
    public class Message
    {
        public Message(string content)
        {
            Content = content;
        }

        public string Content { get; private set; }

    }
}

namespace LognPoolingLib
{
    public class Message()
    {
        public string Сhannel {  get; set; }
        public string User {  get; set; }
        public string Text {  get; set; }

        public static explicit operator MessageDTO(Message message)
        {
            return new MessageDTO()
            {
                Сhannel = message.Сhannel,
                Text = message.Text
            };
        }
    }

    public class MessageDTO()
    {
        public string Сhannel { get; set; }
        public string Text { get; set; }
    }
}

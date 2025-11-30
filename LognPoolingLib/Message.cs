using System.Text.Json.Serialization;

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
                Channel = message.Сhannel,
                Text = message.Text
            };
        }
    }

    public class MessageDTO()
    {
        [JsonPropertyName("Channel")]
        public string Channel { get; set; }

        [JsonPropertyName("Text")]
        public string Text { get; set; }
    }
}

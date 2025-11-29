using LognPoolingLib;
using System.Net.Http;
using System.Text;

namespace ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            /*
            var key = Encoding.UTF8.GetBytes("1111111111111111");
            var iv = Encoding.UTF8.GetBytes("1111111111111111");

            var cripto = new Cripto<MessageDTO>(key, iv);

            var message = new MessageDTO()
            {
                Channel = "Channel1",
                Text = "Text1"
            };

            var smessage = cripto.EncryptObject(message);

            var rmessage = cripto.DecryptObject(smessage);
            */

            var url = "http://localhost:5000/loongPooling";
            var key = Encoding.UTF8.GetBytes("1111111111111111");
            var iv = Encoding.UTF8.GetBytes("1111111111111111");

            var client = new LongPoolingClient(key, iv);

            var headers = new Dictionary<string, string>();
            headers.Add("user", "user1");

            client.SetHeaders(headers);

            client.MessageDelivered += (m) =>
            {
                Console.WriteLine($"{m.Channel} {m.Text}");
            };

            client.SubscribeToChannel("channel1", (m) => Console.WriteLine($"the Сhannel = channel1, {m}"));
            client.SubscribeToChannel("channel2", (m) => Console.WriteLine($"the Сhannel = channel2, {m}"));
            client.SubscribeToChannel("channel3", (m) => Console.WriteLine($"the Сhannel = channel2, {m}"));

            client.Start(url);

            while (true)
            {
                await Task.Delay(1000);
            }
        }
    }
}

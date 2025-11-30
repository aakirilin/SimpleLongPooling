using LognPoolingLib;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            /*
            var url = "http://localhost:5000/loongPooling";

            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            };
            var client = new HttpClient(socketsHandler);

            client.DefaultRequestHeaders.Add("user", "user");

            client
               .DefaultRequestHeaders
               .Accept
               .Add(new MediaTypeWithQualityHeaderValue("text/message"));

            var responce = await client.GetStreamAsync(url);

            var strBuilder = new StringBuilder();
            var key = Encoding.UTF8.GetBytes("1111111111111111");
            var iv = Encoding.UTF8.GetBytes("1111111111111111");
            var cripto = new Cripto<MessageDTO>(key, iv);
            while (true)
            {
                strBuilder.Clear();
                char[] buffer = new char[1024];
                var reader = new StreamReader(responce);

                await reader.ReadBlockAsync(buffer, 0, buffer.Length);
                strBuilder.Append(buffer);
                var messageString = strBuilder.ToString().Trim(new char());

                if (!String.IsNullOrWhiteSpace(messageString))
                {
                    var message = cripto.DecryptObject(messageString);
                }
            }
*/


            var url = "http://localhost:5000/loongPooling";
            var key = Encoding.UTF8.GetBytes("1111111111111111");
            var iv = Encoding.UTF8.GetBytes("1111111111111111");

            var client = new LongPoolingClient(key, iv);

            var headers = new Dictionary<string, string>();
            headers.Add("user", "user");
            headers.Add("Accept", "text/message");

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

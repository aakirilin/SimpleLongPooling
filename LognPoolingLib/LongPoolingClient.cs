using System;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace LognPoolingLib
{
    public class LongPoolingClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private Task bw;
        private CancellationTokenSource cts;

        public event Action<MessageDTO> MessageDelivered;

        private Dictionary<string, EventManager> chanals;
        private byte[] key;
        private byte[] iv;

        public LongPoolingClient(byte[] key, byte[] iv)
        {
            this.httpClient = new HttpClient();
            this.chanals = new Dictionary<string, EventManager>();
            this.key = key;
            this.iv = iv;
        }

        public void SetHeaders(Dictionary<string, string> headers)
        {
            this.httpClient.DefaultRequestHeaders.Clear();
            foreach (var kvp in headers)
            {
                this.httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            }
        }

        public void Start(string baseUrl)
        {
            cts = new CancellationTokenSource();
            bw = new Task(async () =>
            {
                var cripto = new Cripto<MessageDTO>(key, iv);
                var strBuilder = new StringBuilder();
                var responce = await httpClient.GetStreamAsync(baseUrl);
                while (!cts.IsCancellationRequested)
                {
                    try
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
                            OnMessageDelivered(message);
                        }


                    }
                    catch (HttpRequestException ex) { }
                    catch (TaskCanceledException ex) { }
                }
            }, cts.Token);

            bw.Start();
        }

        public void Stop()
        {
            cts.Cancel();
        }

        public void Dispose()
        {
            bw?.Dispose();
        }

        public void OnMessageDelivered(MessageDTO message)
        {
            MessageDelivered?.Invoke(message);
            if (chanals.ContainsKey(message.Channel))
            {
                chanals[message.Channel].CallEvent(message.Text);
            }
        }

        public void SubscribeToChannel(string channeName, Action<string> action)
        {
            if (!chanals.ContainsKey(channeName))
            {
                chanals.Add(channeName, new EventManager());
            }

            chanals[channeName].MessageDelivered += action;
        }

        public void UnsubscribeFromChannel(string channeName, Action<string> action)
        {
            if (chanals.ContainsKey(channeName))
            {
                chanals[channeName].MessageDelivered += action;
            }
        }
    }
}

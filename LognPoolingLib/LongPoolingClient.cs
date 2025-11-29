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
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var cripto = new Cripto<MessageDTO>(key, iv);
                        var stringResponce = await httpClient.GetStringAsync(baseUrl);
                        var message = cripto.DecryptObject(stringResponce);

                        OnMessageDelivered(message);
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

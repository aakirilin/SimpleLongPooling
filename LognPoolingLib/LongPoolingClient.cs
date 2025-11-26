using System.Reflection;
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

        public LongPoolingClient(string user)
        {
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("user", user);
            this.chanals = new Dictionary<string, EventManager>();
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
                        var stringResponce = await httpClient.GetStringAsync(baseUrl);
                        var message = JsonSerializer.Deserialize<MessageDTO>(stringResponce);

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
            if (chanals.ContainsKey(message.Сhannel))
            {
                chanals[message.Сhannel].CallEvent(message.Text);
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

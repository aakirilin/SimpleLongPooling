A simple implementation of long pooling for asp.net with an example. LognPoolingLib is actually the library itself, WebApplicationTestLongPooling is an example of usage.

Usage example:
Creating a client.

```C#
var url = "http://localhost:5000/loongPooling";
var key = Encoding.UTF8.GetBytes("1111111111111111");
var iv = Encoding.UTF8.GetBytes("1111111111111111");
var client = new LongPoolingClient(key, iv);
```

The client is authorized using the request header. It is recommended to put an encrypted token string in this header.
The example shows the simplest authorization, it should not be used in a production environment.

```C#
var headers = new Dictionary<string, string>();
headers.Add("user", "user");
headers.Add("Accept", "text/message");
client.SetHeaders(headers);
```


Client

```C#
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

client.SubscribeToChannel("channel1", (m) => Console.WriteLine($"the Channel = channel1, {m}"));
client.SubscribeToChannel("channel2", (m) => Console.WriteLine($"the Channel = channel2, {m}"));
client.SubscribeToChannel("channel3", (m) => Console.WriteLine($"the Channel = channel2, {m}"));

client.Start(url);

while (true) 
{ 
    await Task.Delay(1000);
}
```

Server: long pooling constroller 
```C#
[Route("[controller]")]
[ApiController]
public class LoongPoolingController : ControllerBase, IDisposable
{
    private readonly MessageQueuePool messageQueuePool;
    UserCancellationTokenSource cancellationTokenSource;
    CancellationTokenSource getCancellationTokenSource;

    public LoongPoolingController(MessageQueuePool message)
    {
        getCancellationTokenSource = new CancellationTokenSource();
        this.messageQueuePool = message;

        this.messageQueuePool.AddMessageEvent += (m) =>
        {
            cancellationTokenSource?.Cancel(m.User);
        };
    }

    [HttpGet]
    public async IAsyncEnumerable<string> GetNew()
    {
        var key = Encoding.UTF8.GetBytes("1111111111111111");
        var iv = Encoding.UTF8.GetBytes("1111111111111111");
        var cripto = new Cripto<MessageDTO>(key, iv);
        var user = HttpContext.Request.Headers["user"];
        cancellationTokenSource = new UserCancellationTokenSource(user);

        while (!getCancellationTokenSource.IsCancellationRequested)
        {
            if (this.HttpContext.RequestAborted.IsCancellationRequested)
            {
                getCancellationTokenSource.Cancel();
            }

            var count = this.messageQueuePool.Count(user);
            if (count > 0)
            {
                var longPoolingServiceMessage = this.messageQueuePool.Dequeue(user);
                var messageDTO = (MessageDTO)longPoolingServiceMessage;

                var encryptObject = cripto.EncryptObject(messageDTO);
                yield return encryptObject.PadRight(1024, new char());                    
            }
            else
            {
                try
                {
                    await Task.Delay(10000, cancellationTokenSource.Token);
                }
                catch (TaskCanceledException e)
                {
                    cancellationTokenSource = new UserCancellationTokenSource(user);
                }
            }                
        }
    }

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        getCancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        getCancellationTokenSource.Dispose();
    }
}
```

Adding a message to the queue
```C#
MessageQueuePool message;
message.Enqueue(new Message() { 
    Channel = "Channel1", 
    Text = "Text", 
    User = "user"
});        
```


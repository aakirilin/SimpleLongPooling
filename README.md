A simple implementation of long pooling for asp.net with an example. LognPoolingLib is actually the library itself, WebApplicationTestLongPooling is an example of usage.


Usage example
Client

```C#
var url = "http://localhost:5000/loongPooling";
var key = Encoding.UTF8.GetBytes("11111111");
var iv = Encoding.UTF8.GetBytes("11111111");

var client = new LongPoolingClient(key, iv);

var headers = new Dictionary<string, string>();
headers.Add("user", "user1");

client.SetHeaders(headers);

client.MessageDelivered += (m) =>
{
    Console.WriteLine($"{m.Channel} {m.Text}");
};

client.SubscribeToChannel("channel1", (m) => Console.WriteLine($"the Ñhannel = channel1, {m}"));
client.SubscribeToChannel("channel2", (m) => Console.WriteLine($"the Ñhannel = channel2, {m}"));
client.SubscribeToChannel("channel3", (m) => Console.WriteLine($"the Ñhannel = channel3, {m}"));

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
public class LoongPoolingController : ControllerBase
{
    private readonly MessageQueuePool messageQueuePool;
    UserCancellationTokenSource cancellationTokenSource;

    public LoongPoolingController(MessageQueuePool message)
    {
        this.messageQueuePool = message;

        this.messageQueuePool.AddMessageEvent += (m) =>
        {
            cancellationTokenSource?.Cancel(m.User);
        };
    }

    [HttpGet]
    public async Task<MessageDTO> Get()
    {
        var key = Encoding.UTF8.GetBytes("11111111");
        var iv = Encoding.UTF8.GetBytes("11111111");
        var cripto = new Cripto<MessageDTO>(key, iv);
        var user = HttpContext.Request.Headers["user"];
        cancellationTokenSource = new UserCancellationTokenSource(user);

        var count = this.messageQueuePool.Count(user);

        Message longPoolingServiceMessage = null;
        MessageDTO messageDTO = null;
        string encryptObject = null;
        if (count > 0)
        {
            longPoolingServiceMessage = this.messageQueuePool.Dequeue(user);
            messageDTO = (MessageDTO)longPoolingServiceMessage;

            encryptObject = cripto.EncryptObject(messageDTO);
            return encryptObject;
        }

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
        }
        catch (TaskCanceledException e) { }

        longPoolingServiceMessage = this.messageQueuePool.Dequeue(user);
        messageDTO = (MessageDTO)longPoolingServiceMessage;

        encryptObject = cripto.EncryptObject(messageDTO);
        return encryptObject;
    }
}
```

Adding a message to the queue
```C#
MessageQueuePool message;
message.Enqueue(new Message() { 
    Channel = "Channel1", 
    Text = "Text", 
    User = "User"
});        
```
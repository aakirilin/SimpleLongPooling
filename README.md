A simple implementation of long pooling for asp.net with an example. LognPoolingLib is actually the library itself, WebApplicationTestLongPooling is an example of usage.


Usage example
Client

```C#
var url = "http://localhost:5076/loongPooling";

var client = new LongPoolingClient("user");

client.MessageDelivered += (m) =>
{
    Console.WriteLine($"{m.Ñhannel} {m.Text}");
};

client.SubscribeToChannel("channel1", (m) => Console.WriteLine($"the Ñhannel = channel1, {m}"));
client.SubscribeToChannel("channel2", (m) => Console.WriteLine($"the Ñhannel = channel2, {m}"));

client.Start(url);
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
        var user = HttpContext.Request.Headers["user"];
        cancellationTokenSource = new UserCancellationTokenSource(user);

        var count = this.messageQueuePool.Count(user);

        Message longPoolingServiceMessage = null;
        if (count > 0)
        {
            longPoolingServiceMessage = this.messageQueuePool.Dequeue(user);
            return (MessageDTO)longPoolingServiceMessage;
        }

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
        }
        catch (TaskCanceledException e) { }

        longPoolingServiceMessage = this.messageQueuePool.Dequeue(user);
        return (MessageDTO)longPoolingServiceMessage;
    }
}
```

Server: long pooling add messge constroller 
```C#
[Route("[controller]")]
[ApiController]
public class AddMessageController : ControllerBase
{
    private readonly MessageQueuePool message;

    public AddMessageController(MessageQueuePool message)
    {
        this.message = message;
    }

    [HttpPost]
    public async Task Post(Reqest reqest)
    {
        message.Enqueue(new Message() { Ñhannel = reqest.Ñhannel, Text = reqest.Message, User = reqest.User });            
    }
}

public class Reqest
{
    public string Ñhannel { get; set; } 
    public string Message { get; set; } 
    public string User { get; set; } 
}
```
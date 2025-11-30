using LognPoolingLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Unicode;
using System.Threading;

namespace WebApplicationTestLongPooling.Controllers
{
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

        [HttpGet("test")]
        public async IAsyncEnumerable<string> GetAsync()
        {
            int i = 0;
            while (true)
            {
                yield return ("test " + i++).PadRight(10);
                await Task.Delay(1000);
            }
        }

        /*
        [HttpGet]
        public async IAsyncEnumerable<string> Get()
        {
            var key = Encoding.UTF8.GetBytes("1111111111111111");
            var iv = Encoding.UTF8.GetBytes("1111111111111111");
            var cripto = new Cripto<MessageDTO>(key, iv);
            var user = HttpContext.Request.Headers["user"];
            cancellationTokenSource = new UserCancellationTokenSource(user);

            var count = this.messageQueuePool.Count(user);

            Message longPoolingServiceMessage = null;
            MessageDTO messageDTO = null;
            string encryptObject = null;
            while (count > 0)
            {
                longPoolingServiceMessage = this.messageQueuePool.Dequeue(user);
                messageDTO = (MessageDTO)longPoolingServiceMessage;

                encryptObject = cripto.EncryptObject(messageDTO);
                yield return encryptObject;

                count = this.messageQueuePool.Count(user);
            }

            try
            {
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException e) { }

            longPoolingServiceMessage = this.messageQueuePool.Dequeue(user);
            messageDTO = (MessageDTO)longPoolingServiceMessage;

            encryptObject = cripto.EncryptObject(messageDTO);
            yield return encryptObject;
        }
        */

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
}

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
        public async Task<string> Get()
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
}

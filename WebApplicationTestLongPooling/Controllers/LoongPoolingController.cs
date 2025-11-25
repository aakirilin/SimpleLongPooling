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
        //private LongPoolingServiceMessage longPoolingServiceMessage;
        private readonly MessageQueuePool message;
        UserCancellationTokenSource cancellationTokenSource;

        public LoongPoolingController(MessageQueuePool message)
        {
            this.message = message;

            this.message.AddMessageEvent += (m) =>
            {
                //this.longPoolingServiceMessage = m;
                cancellationTokenSource?.Cancel(m.User);
            };
        }

        [HttpGet]
        public async Task<MessageDTO> Get()
        {
            var user = HttpContext.Request.Headers["user"];
            cancellationTokenSource = new UserCancellationTokenSource(user);

            var count = this.message.Count(user);

            if (count > 0)
            {
                var messageText = this.message.Dequeue(user);
                return (MessageDTO)messageText;
            }

            try
            {
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException e) { }

            var longPoolingServiceMessage = this.message.Dequeue(user);
            return (MessageDTO)longPoolingServiceMessage;
        }
    }
}

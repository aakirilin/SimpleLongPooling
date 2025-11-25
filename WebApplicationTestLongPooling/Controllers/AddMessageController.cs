using LognPoolingLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplicationTestLongPooling.Controllers
{
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
        public async Task Post(Reqest response)
        {
            message.Enqueue(new Message() { Text = response.Message, User = response.User });            
        }
    }

    public class Reqest
    {
        public string Message { get; set; } 
        public string User { get; set; } 
    }
}

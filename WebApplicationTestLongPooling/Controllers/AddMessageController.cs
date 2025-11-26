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
        public async Task Post(Reqest reqest)
        {
            message.Enqueue(new Message() { Сhannel = reqest.Сhannel, Text = reqest.Message, User = reqest.User });            
        }
    }

    public class Reqest
    {
        public string Сhannel { get; set; } 
        public string Message { get; set; } 
        public string User { get; set; } 
    }
}

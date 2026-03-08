using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using API.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ChatController : BaseController
    {
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;

        public ChatController(IHubContext<ChatHub, IChatClient> chatHub)
        {
            _chatHub = chatHub;
        }

        [HttpPost("messages")]
        public async Task Post(ChatMessage message)
        {
            // run some logic...
            await _chatHub.Clients.All.ReceiveMessage(message);
        }
    }
}

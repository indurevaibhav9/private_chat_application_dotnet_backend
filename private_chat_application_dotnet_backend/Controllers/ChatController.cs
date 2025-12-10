using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using global::private_chat_application_dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;
using private_chat_application_dotnet_backend.Models;
using private_chat_application_dotnet_backend.Services;
using Microsoft.AspNetCore.SignalR;
using private_chat_application_dotnet_backend.Hubs;

namespace private_chat_application_dotnet_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly IHubContext<ChatHub> _hub;
        public ChatController(ChatService chatService, IHubContext<ChatHub> hub)
        {
            _chatService = chatService;
            _hub = hub;
        }

        // GET: api/chat/users/{userId}
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetAllUsersExcept(string userId)
        {
            var users = await _chatService.GetAllUsersExceptAsync(userId);
            return Ok(users);
        }

        // GET: api/chat/messages?senderId=1&receiverId=2
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] string senderId, [FromQuery] string receiverId)
        {
            var messages = await _chatService.GetChatMessagesAsync(senderId, receiverId);
            return Ok(messages);
        }

        // POST: api/chat/send-text
        [HttpPost("send-text")]
        public async Task<IActionResult> SendText([FromBody] TextMessageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var msg = await _chatService.SendTextMessageAsync(dto.SenderId, dto.ReceiverId, dto.Text);
            await _hub.Clients.Group(dto.ReceiverId).SendAsync("ReceiveMessage", msg);
            return Ok(msg);
        }

        // POST: api/chat/send-file
        [HttpPost("send-file")]
        public async Task<IActionResult> SendFile([FromForm] FileMessageDto dto)
        {
            if (dto.File == null)
                return BadRequest("File is required.");

            var msg = await _chatService.SendFileMessageAsync(
                dto.File,
                dto.SenderId,
                dto.ReceiverId,
                dto.MessageType
            );
            await _hub.Clients.Group(dto.ReceiverId).SendAsync("ReceiveMessage", msg);
            return Ok(msg);
        }


        // PATCH: api/chat/mark-seen?userId=1&senderId=2
        [HttpPatch("mark-seen")]
        public async Task<IActionResult> MarkSeen([FromQuery] string userId, [FromQuery] string senderId)
        {
            await _chatService.MarkAsSeenAsync(userId, senderId);
            return Ok(new { success = true });
        }
    }

    // DTO for sending text message
    public class TextMessageDto
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Text { get; set; }
    }

    public class FileMessageDto
    {
        public IFormFile File { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string MessageType { get; set; } = "file";
    }

}

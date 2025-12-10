using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using private_chat_application_dotnet_backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace private_chat_application_dotnet_backend.Services
{
    public class ChatService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Message> _messages;
        private readonly ICloudinaryService? _cloudinary;

        public ChatService(IConfiguration config, ICloudinaryService? cloudinary = null)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDB:Database"]);

            _users = db.GetCollection<User>("Users");
            _messages = db.GetCollection<Message>("Messages");
            _cloudinary = cloudinary;
        }

        // Returns all users except the provided userId
        public async Task<List<User>> GetAllUsersExceptAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            return await _users.Find(u => u.Id != userId).ToListAsync();
        }

        // Returns the conversation between two users (sorted by timestamp ascending)
        public async Task<List<Message>> GetChatMessagesAsync(string senderId, string receiverId)
        {
            if (string.IsNullOrWhiteSpace(senderId)) throw new ArgumentNullException(nameof(senderId));
            if (string.IsNullOrWhiteSpace(receiverId)) throw new ArgumentNullException(nameof(receiverId));

            return await _messages.Find(m =>
                    (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                    (m.SenderId == receiverId && m.ReceiverId == senderId))
                .SortBy(m => m.Timestamp)
                .ToListAsync();
        }

        // Send a text message
        public async Task<Message> SendTextMessageAsync(string senderId, string receiverId, string text)
        {
            if (string.IsNullOrWhiteSpace(senderId)) throw new ArgumentNullException(nameof(senderId));
            if (string.IsNullOrWhiteSpace(receiverId)) throw new ArgumentNullException(nameof(receiverId));
            if (text is null) throw new ArgumentNullException(nameof(text));

            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageType = "text",
                Text = text,
                IsSeen = false,
                Timestamp = DateTime.UtcNow
            };

            await _messages.InsertOneAsync(msg);
            return msg;
        }

        // Send a file/image message using Cloudinary (Cloudinary service is optional but recommended)
        public async Task<Message> SendFileMessageAsync(IFormFile file, string senderId, string receiverId, string messageType = "file")
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrWhiteSpace(senderId)) throw new ArgumentNullException(nameof(senderId));
            if (string.IsNullOrWhiteSpace(receiverId)) throw new ArgumentNullException(nameof(receiverId));
            if (_cloudinary == null) throw new InvalidOperationException("Cloudinary service is not configured.");

            var (url, name) = await _cloudinary.UploadFileAsync(file, "chat");
            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageType = messageType,
                FileUrl = url,
                FileName = name,
                IsSeen = false,
                Timestamp = DateTime.UtcNow
            };

            await _messages.InsertOneAsync(msg);
            return msg;
        }

        // Mark all unseen messages from senderId -> userId as seen
        public Task MarkAsSeenAsync(string userId, string senderId)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(senderId)) throw new ArgumentNullException(nameof(senderId));

            var update = Builders<Message>.Update.Set(m => m.IsSeen, true);
            return _messages.UpdateManyAsync(
                m => m.SenderId == senderId && m.ReceiverId == userId && !m.IsSeen,
                update
            );
        }
    }
}

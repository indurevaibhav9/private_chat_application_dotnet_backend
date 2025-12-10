using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace private_chat_application_dotnet_backend.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SenderId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ReceiverId { get; set; }

        public string MessageType { get; set; } // text | image | file
        public string? Text { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }

        public bool IsSeen { get; set; } = false;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

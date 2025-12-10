using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace private_chat_application_dotnet_backend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }
        public string Mobile { get; set; }
        public string PasswordHash { get; set; }
        public string AvatarUrl { get; set; }

        public string OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    }
}

using MongoDB.Driver;
using private_chat_application_dotnet_backend.Models;

public class MessageRepository
{
    private readonly IMongoCollection<Message> _messages;

    public MessageRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var db = client.GetDatabase(config["MongoDB:Database"]);
        _messages = db.GetCollection<Message>("Messages");
    }

    public async Task<Message> SendMessage(Message message)
    {
        await _messages.InsertOneAsync(message);
        return message;
    }

    public async Task<List<Message>> GetConversation(string senderId, string receiverId)
    {
        return await _messages
            .Find(m =>
                (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                (m.SenderId == receiverId && m.ReceiverId == senderId))
            .SortBy(m => m.Timestamp)
            .ToListAsync();
    }

    public Task MarkAsSeen(string userId, string senderId) =>
        _messages.UpdateManyAsync(
            m => m.SenderId == senderId && m.ReceiverId == userId && !m.IsSeen,
            Builders<Message>.Update.Set(m => m.IsSeen, true)
        );
}


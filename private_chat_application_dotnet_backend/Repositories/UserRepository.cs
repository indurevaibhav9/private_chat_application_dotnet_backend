using MongoDB.Driver;
using private_chat_application_dotnet_backend.Models;

public class UserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var db = client.GetDatabase(config["MongoDB:Database"]);
        _users = db.GetCollection<User>("Users");
    }

    public Task<User> GetByMobileAsync(string mobile) =>
        _users.Find(u => u.Mobile == mobile).FirstOrDefaultAsync();

    public Task<User> GetByIdAsync(string id) =>
        _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User user) =>
        await _users.InsertOneAsync(user);

    public Task<List<User>> GetAllExceptAsync(string userId) =>
        _users.Find(u => u.Id != userId).ToListAsync();

    public Task UpdateAsync(User user) =>
        _users.ReplaceOneAsync(x => x.Id == user.Id, user);
}

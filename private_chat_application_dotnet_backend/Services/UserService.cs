namespace private_chat_application_dotnet_backend.Services
{
    using BCrypt.Net;
    using MongoDB.Driver;
    using private_chat_application_dotnet_backend.Models;

    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDB:Database"]);
            _userCollection = db.GetCollection<User>(config["MongoDB:UserCollection"]);
        }

        public async Task<User?> GetByMobileAsync(string mobile) =>
            await _userCollection.Find(u => u.Mobile == mobile).FirstOrDefaultAsync();

        public async Task<User> CreateAsync(string name, string mobile, string password)
        {
            var user = new User
            {
                Name = name,
                Mobile = mobile,
                PasswordHash = BCrypt.HashPassword(password)
            };

            await _userCollection.InsertOneAsync(user);
            return user;
        }

        public async Task<User?> AuthenticateUser(string mobile, string password)
        {
            // Find user by mobile number
            var user = await _userCollection
                            .Find(u => u.Mobile == mobile)
                            .FirstOrDefaultAsync();

            // If no user found, return null
            if (user == null)
                return null;

            // Verify password
            bool isPasswordValid = BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
                return null;

            return user; // Authentication successful
        }

    }

}

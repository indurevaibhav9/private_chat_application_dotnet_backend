namespace private_chat_application_dotnet_backend.Infrastructure
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UserCollection { get; set; }
        public string OtpCollection { get; set; }
    }

}

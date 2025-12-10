namespace private_chat_application_dotnet_backend.Models
{
    using MongoDB.Bson;

    public class OtpRecord
    {
        public ObjectId Id { get; set; }
        public string Mobile { get; set; }
        public string Otp { get; set; }
        public DateTime Expiry { get; set; }
    }

}

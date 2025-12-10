using MongoDB.Driver;
using private_chat_application_dotnet_backend.Models;

namespace private_chat_application_dotnet_backend.Services
{
    public class OtpService
    {
        private readonly IMongoCollection<OtpRecord> _otpCollection;

        public OtpService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDB:Database"]);
            _otpCollection = db.GetCollection<OtpRecord>(config["MongoDB:OtpCollection"]);
        }

        public async Task<string> GenerateOtpAsync(string mobile)
        {
            // Make sure only latest OTP exists
            await _otpCollection.DeleteManyAsync(x => x.Mobile == mobile);

            var otp = new Random().Next(100000, 999999).ToString();

            var record = new OtpRecord
            {
                Mobile = mobile,
                Otp = otp,
                Expiry = DateTime.UtcNow.AddMinutes(5)
            };

            await _otpCollection.InsertOneAsync(record);

            return otp;
        }

        public async Task<bool> ValidateOtpAsync(string mobile, string otp)
        {
            // Always fetch latest OTP (in case multiple somehow exist)
            var record = await _otpCollection.Find(x => x.Mobile == mobile)
                                             .SortByDescending(x => x.Expiry)
                                             .FirstOrDefaultAsync();

            if (record == null || record.Otp != otp)
                return false;

            Console.WriteLine($"{DateTime.UtcNow} (Now) , {record.Expiry} (Expiry)");

            var isValid = record.Expiry > DateTime.UtcNow;

            // Delete OTP after successful use (one-time use)
            if (isValid)
                await _otpCollection.DeleteOneAsync(x => x.Id == record.Id);

            return isValid;
        }
    }
}

namespace private_chat_application_dotnet_backend.Dtos
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; } // optional
    }

    public class LoginDto
    {
        public string Mobile { get; set; }
    }

    public class VerifyOtpDto
    {
        public string Mobile { get; set; }
        public string Otp { get; set; }
    }
}

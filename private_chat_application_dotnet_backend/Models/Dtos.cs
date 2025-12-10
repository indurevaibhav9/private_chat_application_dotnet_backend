namespace private_chat_application_dotnet_backend.Models
{
    public record RegisterDto(string Name, string Mobile, string Password);
    public record LoginDto(string Mobile);
    public record VerifyOtpDto(string Mobile, string Otp);
    public record LoginWtihPasswrdDto (string Mobile, string Password);

}

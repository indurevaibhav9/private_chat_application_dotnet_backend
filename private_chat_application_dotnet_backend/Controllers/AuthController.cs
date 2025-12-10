using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using private_chat_application_dotnet_backend.Hubs;
using private_chat_application_dotnet_backend.Models;
using private_chat_application_dotnet_backend.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly OtpService _otpService;
    private readonly JwtService _jwt;
    private readonly IHubContext<ChatHub> _hub;
    public AuthController(UserService userService, OtpService otpService, JwtService jwt, IHubContext<ChatHub> hub)
    {
        _userService = userService;
        _otpService = otpService;
        _jwt = jwt;
        _hub = hub;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var exists = await _userService.GetByMobileAsync(dto.Mobile);
        if (exists != null) return BadRequest("User already exists");

        var user = await _userService.CreateAsync(dto.Name, dto.Mobile, dto.Password);
        await _hub.Clients.All.SendAsync("NewUserRegistered", user);

        return Ok(user);
    }

    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp(LoginDto dto)
    {
        var user = await _userService.GetByMobileAsync(dto.Mobile);
        if (user == null) return NotFound("User not found");

        var otp = await _otpService.GenerateOtpAsync(dto.Mobile);
        Console.WriteLine($"DEBUG OTP: {otp}");
        return Ok("OTP sent.");
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
    {
        if (!await _otpService.ValidateOtpAsync(dto.Mobile, dto.Otp))
            return Unauthorized($"Invalid OTP ");

        var user = await _userService.GetByMobileAsync(dto.Mobile);
        var token = _jwt.GenerateToken(user.Id.ToString(), user.Mobile);

        return Ok(new { token, user });
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginWithPassword(LoginWtihPasswrdDto dto)
    {
        User user = await _userService.AuthenticateUser(dto.Mobile, dto.Password);
        if (user == null) return Unauthorized("Invalid mobile or password");
        var token = _jwt.GenerateToken(user.Id.ToString(), user.Mobile);
        return Ok(new { token, user });
    }
}

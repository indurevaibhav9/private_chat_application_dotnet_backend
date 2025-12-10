using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using private_chat_application_dotnet_backend.Hubs;
using private_chat_application_dotnet_backend.Infrastructure;
using private_chat_application_dotnet_backend.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var mongoConfig = builder.Configuration.GetSection("MongoDb");
var jwtConfig = builder.Configuration.GetSection("JWT");
var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");

// DI Config
builder.Services.Configure<MongoDbSettings>(mongoConfig);
builder.Services.Configure<JwtSettings>(jwtConfig);
builder.Services.Configure<CloudinarySettings>(cloudinaryConfig);

builder.Services.AddSingleton(sp =>
    new MongoClient(mongoConfig.GetValue<string>("ConnectionString"))
);

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<OtpService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<ChatService>();

// JWT Auth (single valid block)
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        // allow token via query string for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddSingleton(sp =>
{
    var settings = MongoClientSettings.FromConnectionString(
        builder.Configuration["MongoDB:ConnectionString"]);

    settings.Credential = MongoCredential.CreatePlainCredential(
        builder.Configuration["MongoDB:Database"],
        "indurevaibhav9",
        "Vaibhav18"
    );

    return new MongoClient(settings);
});

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow CORS (required for frontend like React/Angular)
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("FrontendPolicy", policy =>
//    {
//        policy
//            .WithOrigins("http://localhost:5173") // add your frontend URLs here
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost") ||
                origin.Contains("ngrok-free.app")
            );
    });
});



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors(policy =>
    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)
);
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("chatHub");

app.Run();

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using private_chat_application_dotnet_backend.Infrastructure;


namespace private_chat_application_dotnet_backend.Services
{
    
    public interface ICloudinaryService
    {
        Task<(string url, string name)> UploadFileAsync(IFormFile file, string folder = "chat");
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _client;
        public CloudinaryService(IOptions<CloudinarySettings> cfg)
        {
            var s = cfg.Value;
            _client = new Cloudinary(new Account(s.CloudName, s.ApiKey, s.ApiSecret));
        }

        public async Task<(string url, string name)> UploadFileAsync(IFormFile file, string folder = "chat")
        {
            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder,
                PublicId = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.UtcNow.Ticks}"
            };

            // If image, you may prefer ImageUploadParams
            var result = await _client.UploadAsync(uploadParams);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
                return (result.SecureUrl.ToString(), result.PublicId);
            throw new Exception("Upload failed");
        }
    }

}

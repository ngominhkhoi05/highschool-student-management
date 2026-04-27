namespace highschool_student_management.Services
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
    }

    public interface ICloudinaryService
    {
        Task<string?> UploadImageAsync(IFormFile file, string fileNamePrefix);
        Task<bool> DeleteImageAsync(string publicId);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CloudinaryService(CloudinarySettings settings)
        {
            var account = new CloudinaryDotNet.Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret);
            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string fileNamePrefix)
        {
            if (file == null || file.Length == 0)
                return null;

            // Tao public ID de lay duong dan URL
            var publicId = $"{fileNamePrefix}_{Guid.NewGuid():N}";

            using var stream = file.OpenReadStream();
            var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams
            {
                File = new CloudinaryDotNet.FileDescription(publicId, stream),
                PublicId = publicId,
                Transformation = new CloudinaryDotNet.Transformation()
                    .Width(300).Height(300)
                    .Crop("fill")
                    .Gravity("face")
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return result.SecureUrl.ToString();
            }

            return null;
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            var deleteParams = new CloudinaryDotNet.Actions.DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}

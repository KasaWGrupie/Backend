using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KasaWGrupie.Infrastructure.ImageService
{
    public class GcsImageOptions
    {
        // must match your .env variable name, or the JSON section name
        public string BucketName { get; set; } = default!;
        // optional: if you want to explicitly load a creds file
        public string? CredentialsPath { get; set; }
    }
    public class GcsImageService : IImageService
    {
        private string _bucketName;
        private StorageClient _storageClient;

        
        public GcsImageService(IOptions<GcsImageOptions> optionsAccessor, StorageClient storageClient)
        {
            _storageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));
            var opts = optionsAccessor.Value;
            _bucketName =
            !string.IsNullOrWhiteSpace(opts.BucketName)
                ? opts.BucketName
                : throw new ArgumentNullException(nameof(opts.BucketName), "BucketName is required");
            
        }

        public async Task<UploadResult> UploadImageAsync(IFormFile image, CancellationToken cancellationToken)
        {
            if (image == null || image.Length == 0)
            {
                return new UploadResult { IsSuccess = false, Errors = { "Invalid image file." } };
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            try
            {
                using var stream = image.OpenReadStream();
                await _storageClient.UploadObjectAsync(_bucketName, fileName, image.ContentType, stream, cancellationToken: cancellationToken);

                string fileUrl = $"https://storage.googleapis.com/{_bucketName}/{fileName}";

                return new UploadResult { IsSuccess = true, Url = fileUrl };
            }
            catch (Exception ex)
            {
                return new UploadResult { IsSuccess = false, Errors = { $"Error uploading image: {ex.Message}" } };
            }
        }
    }
}

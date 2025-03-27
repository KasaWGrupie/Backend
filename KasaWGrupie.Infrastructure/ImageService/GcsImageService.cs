using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KasaWGrupie.Infrastructure.ImageService
{
    public class GcsImageService : IImageService
    {
        private string _bucketName;
        private StorageClient _storageClient;

        
        public GcsImageService(IConfiguration configuration, StorageClient storageClient)
        {
            _storageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));
            _bucketName = configuration["GoogleCloud:BucketName"]
         ?? throw new ArgumentNullException(nameof(_bucketName), "BucketName is required");
            
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

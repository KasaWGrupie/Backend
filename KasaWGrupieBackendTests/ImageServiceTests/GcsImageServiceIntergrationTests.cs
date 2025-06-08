using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using DotNetEnv;
using KasaWGrupie.Infrastructure.ImageService;

namespace KasaWGrupieBackendTests.IntegrationTests
{
    [TestClass]
    public class GcsImageServiceTests
    {
        private GcsImageService _imageService;
        private string _bucketName;

        [TestInitialize]
        public void Setup()
        {
            Env.TraversePath().Load();
            _bucketName = Environment.GetEnvironmentVariable("GCP_BUCKET_NAME")
                ?? throw new InvalidOperationException("BucketName is missing in secrets.");
            
            var credentialsPath = Environment.GetEnvironmentVariable("GCP_CREDENTIALS_PATH")
                            ?? throw new InvalidOperationException("CredentialsPath is missing in secrets.");
            
                        // 2) Build the real StorageClient
            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(credentialsPath);
            var storageClient = StorageClient.Create(credential);
            
                        // 3) Wrap your two needed settings into IOptions<GcsImageOptions>
            var opts = new GcsImageOptions
                        {
                BucketName = _bucketName,
CredentialsPath = credentialsPath  // optional in the service itself
            };
            var options = Options.Create(opts);
            
                        // 4) New constructor signature
            _imageService = new GcsImageService(options, storageClient);
        


        }

        [TestMethod]
        public async Task UploadImageAsync_ShouldReturnSuccess_WhenUploadIsSuccessful()
        {
            // Arrange: Create a fake test file
            var testFileName = "test-image.jpg";
            var testImagePath = Path.Combine(Directory.GetCurrentDirectory(), testFileName);

            if (!File.Exists(testImagePath))
            {
                await File.WriteAllTextAsync(testImagePath, "FakeImageData");
            }

            await using var stream = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);
            var formFile = new FormFile(stream, 0, stream.Length, "testFile", testFileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var cancellationToken = CancellationToken.None;

            // Act: Call the real Google Cloud Storage
            var result = await _imageService.UploadImageAsync(formFile, cancellationToken);

            // Assert: Check if the upload was successful
            result.IsSuccess.Should().BeTrue();
            result.Url.Should().NotBeNullOrEmpty();
            result.Url.Should().Contain(_bucketName);
        }
    }
}

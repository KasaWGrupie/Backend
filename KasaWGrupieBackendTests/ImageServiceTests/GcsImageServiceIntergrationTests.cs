using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
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
            // Load secrets configuration
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<GcsImageServiceTests>()
                .Build();
            // Debugging output
            Console.WriteLine("GoogleCloud:BucketName = " + configuration["GoogleCloud:BucketName"]);
            Console.WriteLine("GoogleCloud:CredentialsPath = " + configuration["GoogleCloud:CredentialsPath"]);


            _bucketName = configuration["GoogleCloud:BucketName"]
                ?? throw new InvalidOperationException("BucketName is missing in secrets.");

            var credentialsPath = configuration["GoogleCloud:CredentialsPath"]
                ?? throw new InvalidOperationException("CredentialsPath is missing in secrets.");

            // Authenticate and create StorageClient
            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(credentialsPath);
            var storageClient = StorageClient.Create(credential);

            // Create the real service
            _imageService = new GcsImageService(configuration, storageClient);
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

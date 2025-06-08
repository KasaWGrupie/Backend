using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using KasaWGrupie.Infrastructure.ImageService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;


namespace KasaWGrupie.Tests.ImageServiceTests
{
    [TestClass]
    public class GcsImageServiceTests
    {
        private Mock<StorageClient> _storageClientMock;
        private GcsImageService _imageService;
        private IOptions<GcsImageOptions> _options;

        [TestInitialize]
        public void Setup()
        {
            _storageClientMock = new Mock<StorageClient>();

            var opts = new GcsImageOptions
            {
                BucketName = "test-bucket",
                CredentialsPath = null      // or some dummy path
            };
            _options = Options.Create(opts);

            // 3) Pass the options + mock client into your service
            _imageService = new GcsImageService(_options, _storageClientMock.Object);
        }

        [TestMethod]
        public async Task UploadImageAsync_ShouldReturnSuccess_WhenUploadIsSuccessful()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.jpg";
            var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(fileStream.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
           

          

            _storageClientMock
    .Setup(client => client.UploadObjectAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<Stream>(),
        It.IsAny<UploadObjectOptions>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<IProgress<Google.Apis.Upload.IUploadProgress>>()))
    .ReturnsAsync(new Google.Apis.Storage.v1.Data.Object { Name = "fake-image.jpg" }); //Ensure correct return type




            // Act
            var result = await _imageService.UploadImageAsync(fileMock.Object, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Url.StartsWith("https://storage.googleapis.com/test-bucket/"));
        }

        [TestMethod]
        public async Task UploadImageAsync_ShouldReturnError_WhenImageIsNull()
        {
            // Act
            var result = await _imageService.UploadImageAsync(null, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Invalid image file.", result.Errors[0]);
        }
    }
}

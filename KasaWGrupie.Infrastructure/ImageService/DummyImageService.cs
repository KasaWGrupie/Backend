using Microsoft.AspNetCore.Http;

namespace KasaWGrupie.Infrastructure.ImageService;

public class DummyImageService : IImageService
{
	public Task<UploadResult> UploadImageAsync(IFormFile image, CancellationToken cancellationToken)
	{
		return Task.FromResult(UploadResult.Success("https://dummyimage.com/600x400/000/fff"));
	}
}

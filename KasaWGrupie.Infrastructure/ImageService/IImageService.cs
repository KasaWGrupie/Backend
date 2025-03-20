using Microsoft.AspNetCore.Http;

namespace KasaWGrupie.Infrastructure.ImageService;

public interface IImageService
{
	public Task<UploadResult> UploadImageAsync(IFormFile image, CancellationToken cancellationToken);
}
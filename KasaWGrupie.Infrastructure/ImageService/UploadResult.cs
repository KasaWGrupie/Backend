namespace KasaWGrupie.Infrastructure.ImageService;

public class UploadResult
{
	public bool IsSuccess { get; set; }
	public string? Url { get; set; }
	public List<string> Errors { get; set; }

	public UploadResult()
	{
		Errors = new List<string>();
	}

	public static UploadResult Success(string url)
	{
		return new UploadResult
		{
			IsSuccess = true,
			Url = url
		};
	}

	public static UploadResult Failure(IEnumerable<string> errors)
	{
		return new UploadResult
		{
			IsSuccess = false,
			Errors = errors.ToList()
		};
	}
}
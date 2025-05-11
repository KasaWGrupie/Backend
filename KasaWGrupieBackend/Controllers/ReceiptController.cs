using System.Threading.Tasks;
using KasaWGrupie.Core;
using KasaWGrupie.Infrastructure.ReceiptProcessor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KasaWGrupie.API.Controllers
{
    [ApiController]
    [Route("api/receipts")]               // Base path for all receipt endpoints
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptProcessor _processor;

        // ASP.NET Core will inject your AzureReceiptProcessor here
        public ReceiptController(IReceiptProcessor processor)
            => _processor = processor;

        /// <summary>
        /// Analyze an uploaded receipt image and return parsed items and total.
        /// </summary>
        [HttpPost("analyze")]             // POST /api/receipts/analyze
        [ProducesResponseType(typeof(ReceiptParseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Analyze([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please attach a non-empty receipt file.");

            using var stream = file.OpenReadStream();

            ReceiptParseResult result;
            try
            {
                result = await _processor.AnalyzeAsync(stream);
            }
            catch (Azure.RequestFailedException ex)
            {
                // Model couldn’t parse it—bad content or corrupted
                return BadRequest(new { error = ex.Message });
            }

            return Ok(result);
        }
    }
}

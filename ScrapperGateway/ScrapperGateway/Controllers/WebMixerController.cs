using Microsoft.AspNetCore.Mvc;
using ServicesLayer;
using System.Text.Json;

namespace ScrapperGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebMixerController : ControllerBase
    {
        private readonly IWebMixerService _webMixerService;

        public WebMixerController(IWebMixerService webMixerService)
        {
            _webMixerService = webMixerService;
        }
        [HttpGet("GetBestAds")]
        public async Task<IActionResult> GetBestAds(string keywords, string userSearch, int pagestoscrape, int? category,string? latitude, string? longitude, int? minprice, int? maxprice, int? brandId, int? modelId)
        {
            var adsList = await _webMixerService.AnalyzeAds(keywords, userSearch, pagestoscrape, category, latitude, longitude, minprice, maxprice, brandId, modelId);

            if (adsList == null || !adsList.Any())
            {
                return NotFound("No se encontraron resultados.");
            }

            var jsonResponse = JsonSerializer.Serialize(adsList);

            return Content(jsonResponse, "application/json");
        }


    }
}

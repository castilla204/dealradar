using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClientScrapperMilanuncios.Models;
using ClientScrapperMilanuncios.DataLayer;

namespace ClientScrapperMilanuncios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MilAdsController : ControllerBase
    {
        private readonly MilAdsClientData _milAdsClient;

        public MilAdsController(MilAdsClientData wallaData)
        {
            _milAdsClient = wallaData;
        }







        public class ScrapeRequest
        {
            public List<string> SearchTerms { get; set; }
            public int PagesToScrape { get; set; }
        }

        [HttpPost("scrape")]
        public async Task<IActionResult> ScrapeAds([FromBody] ScrapeRequest request)
        {
            try
            {
                await _milAdsClient.DisplayMessageAsync(request.SearchTerms, request.PagesToScrape);
                return Ok("Scraping completado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }







    }
}

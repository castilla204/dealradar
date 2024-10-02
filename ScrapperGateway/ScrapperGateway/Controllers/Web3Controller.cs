using Microsoft.AspNetCore.Mvc;
using ServicesLayer;

namespace ScrapperGateway.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class Web3Controller : ControllerBase
    {

        private readonly IWeb3Service _web3Service;

        public Web3Controller(IWeb3Service web3Service)
        {
            _web3Service = web3Service;
        }



        [HttpGet("clothesScrapper")]
        public async Task<IActionResult> GetCarListings(string keywords, string? latitude, string? longitude, int? minprice, int? maxprice)
        {
            var jsonResponse = await _web3Service.GetWallapop(keywords, latitude, longitude, minprice, maxprice);

            if (string.IsNullOrEmpty(jsonResponse))
            {
                return NotFound("No se encontraron resultados.");
            }

            return Content(jsonResponse, "application/json");
        }



    }
}

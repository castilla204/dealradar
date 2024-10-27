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
        public async Task<IActionResult> GetCarListings(string keywords, int pagestoscrap, int? category, string? latitude, string? longitude, int? minprice, int? maxprice)
        {
            // Agregar registros para la depuración
            Console.WriteLine($"Received keywords: {keywords}, latitude: {latitude}, longitude: {longitude}, minprice: {minprice}, maxprice: {maxprice}");

            var jsonResponse = await _web3Service.GetWallapop(keywords, pagestoscrap, category,  latitude, longitude, minprice, maxprice);

            // Comprobar si la respuesta JSON está vacía o es un array vacío
            if (string.IsNullOrEmpty(jsonResponse) || jsonResponse == "[]")
            {
                return NotFound("No se encontraron resultados.");
            }

            return Content(jsonResponse, "application/json");
        }
    }
}

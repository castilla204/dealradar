using Microsoft.AspNetCore.Mvc;
using ServicesLayer;

namespace ScrapperGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Web4Controller : ControllerBase
    {
        private readonly IWeb4Service _web4Service;

        public Web4Controller(IWeb4Service web4Service)
        {
            _web4Service = web4Service;
        }

        [HttpGet("clothesScrapper")]
        public async Task<IActionResult> GetCarListings(string keywords)
        {


            var jsonResponse = await _web4Service.GetWallapop(keywords);

            // Comprobar si la respuesta JSON está vacía o es un array vacío
            if (string.IsNullOrEmpty(jsonResponse) || jsonResponse == "[]")
            {
                return NotFound("No se encontraron resultados.");
            }

            return Content(jsonResponse, "application/json");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using ServicesLayer;

namespace ScrapperGateway.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class Web2Controller : ControllerBase
    {

        private readonly IWeb2Service _web2Service;

        public Web2Controller(IWeb2Service web2Service)
        {
            _web2Service = web2Service;
        }



        [HttpGet("clothesScrapper")]
        public async Task<IActionResult> GetCarListings(string searchKey)
        {
            var jsonResponse = await _web2Service.GetVintedList(searchKey);

            if (string.IsNullOrEmpty(jsonResponse))
            {
                return NotFound("No se encontraron resultados.");
            }

            return Content(jsonResponse, "application/json");
        }



    }
}

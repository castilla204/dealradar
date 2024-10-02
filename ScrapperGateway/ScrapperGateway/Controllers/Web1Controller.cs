using Microsoft.AspNetCore.Mvc;
using ServicesLayer;

namespace ScrapperGateway.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class Web1Controller : ControllerBase
    {

        private readonly IWeb1Service _web1Service;

        public Web1Controller(IWeb1Service web1Service)
        {
            _web1Service = web1Service;
        }



        [HttpGet("carlistings")]
        public async Task<IActionResult> GetCarListings(int brandId, int modelId)
        {
            var jsonResponse = await _web1Service.GetCarList(brandId, modelId);

            if (string.IsNullOrEmpty(jsonResponse))
            {
                return NotFound("No se encontraron resultados.");
            }

            return Content(jsonResponse, "application/json");
        }





    }
}

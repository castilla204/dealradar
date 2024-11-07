using Microsoft.AspNetCore.Mvc;
using ServicesLayer;
using DataLayer.Models; // Asegúrate de incluir el espacio de nombres para AdModel
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var adModels = await _web4Service.GetWallapop(keywords);

            // Comprobar si la lista está vacía
            if (adModels == null || adModels.Count == 0)
            {
                return NotFound("No se encontraron resultados.");
            }

            // Retornar la lista de anuncios como JSON
            return Ok(adModels);
        }
    }
}

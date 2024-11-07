using AutoMapper;
using DataLayer;
using DataLayer.Models;

namespace ServicesLayer
{
    public class Web4Service : IWeb4Service
    {
        private readonly IWeb4Data _web4Data;

        public Web4Service(IWeb4Data web4Data)
        {
            _web4Data = web4Data;
        }

        // La palabra clave async debe ir antes del tipo de retorno
        public async Task<List<AdModel>> GetWallapop(string keywords)
        {
            return await _web4Data.GetAnunciosAsync(keywords);
        }
    }
}

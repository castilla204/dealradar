using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Mapping;
using MongoDB.Driver;
using AutoMapper;
using DataLayer.Models.Wallapop;
using DataLayer.Models;

namespace DataLayer
{
    public class Web4Data : IWeb4Data
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Root> _anuncios;

        public Web4Data(IMapper mapper, IMongoCollection<Root> anuncios)
        {
            _mapper = mapper;
            _anuncios = anuncios;
        }

        // Clase contenedora para manejar el JSON con la propiedad "data"
        public class DataWrapper<T>
        {
            public List<T> data { get; set; }
        }

        public async Task<List<AdModel>> GetAnunciosAsync(string keyword)
        {
            try
            {
                var filter = Builders<Root>.Filter.Or(
                    Builders<Root>.Filter.Regex(x => x.title, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    Builders<Root>.Filter.Regex(x => x.description, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
                );

                var resultados = await _anuncios.Find(filter).ToListAsync();

                // Mapeo a AdModel
                var adModels = _mapper.Map<List<AdModel>>(resultados);

                // Convertir los resultados mapeados a JSON dentro de un objeto que tenga la propiedad "data"
                return adModels;// Si necesitas que sea un JSON en string
            }
            catch (Exception ex)
            {
                throw new Exception("Error al deserializar los anuncios", ex);
            }
        }
    }
}

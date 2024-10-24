using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using AutoMapper;
using DataLayer.Models.Wallapop;

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

        public async Task<string> MakeRequestAsync(string keywords)
        {
            var filter = Builders<Root>.Filter.Or(
                Builders<Root>.Filter.Regex(x => x.title, new MongoDB.Bson.BsonRegularExpression(keywords, "i")),
                Builders<Root>.Filter.Regex(x => x.description, new MongoDB.Bson.BsonRegularExpression(keywords, "i"))
            );

            var resultados = await _anuncios.Find(filter).ToListAsync();

            // Convertir los resultados a JSON dentro de un objeto que tenga la propiedad "data"
            var jsonResultados = JsonConvert.SerializeObject(new { data = resultados }, Formatting.Indented);

            return jsonResultados;
        }

        public async Task<List<Root>> GetAnunciosAsync(string keyword)
        {
            try
            {
                string jsonResponse = await MakeRequestAsync(keyword);

                // Cambiar a JsonConvert para deserializar
                var dataWrapper = JsonConvert.DeserializeObject<DataWrapper<Root>>(jsonResponse);

                return dataWrapper?.data ?? new List<Root>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al deserializar los anuncios", ex);
            }
        }
    }
}

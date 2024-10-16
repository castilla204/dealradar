using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using DataLayer.Models.Wallapop;
using AutoMapper;
using MongoDB.Driver;

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

        public async Task<string> MakeRequestAsync(string keywords)
        {
            var filter = Builders<Root>.Filter.Or(
                Builders<Root>.Filter.Regex(x => x.title, new MongoDB.Bson.BsonRegularExpression(keywords, "i")),
                Builders<Root>.Filter.Regex(x => x.description, new MongoDB.Bson.BsonRegularExpression(keywords, "i"))
            );

            var resultados = await _anuncios.Find(filter).ToListAsync();

            // Convertir los resultados a JSON
            var jsonResultados = JsonConvert.SerializeObject(new { data = resultados }, Formatting.Indented);

            return jsonResultados;
        }
    }
}
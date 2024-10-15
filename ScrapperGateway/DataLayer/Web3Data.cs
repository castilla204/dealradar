using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using ScrapperGateway.Models.Wallapop;
using AutoMapper;

namespace DataLayer
{
    public class Web3Data : IWeb3Data
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string deviceId;
        private readonly string mpid;
        private const string APP_VERSION = "83070";
        private readonly IMapper _mapper;

        public Web3Data(IMapper mapper)
        {
            _mapper = mapper;
            deviceId = Guid.NewGuid().ToString();
            mpid = GenerateMPID();
            SetupHttpClient();
        }

        private string GenerateMPID()
        {
            Random random = new Random();
            return (8000000000000000000 + random.Next(1999999999)).ToString();
        }

        private void SetupHttpClient()
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("Accept-Language", "es,es-ES;q=0.9");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("DeviceOS", "0");
            client.DefaultRequestHeaders.Add("MPID", mpid);
            client.DefaultRequestHeaders.Add("Origin", "https://es.wallapop.com");
            client.DefaultRequestHeaders.Add("Pragma", "no-cache");
            client.DefaultRequestHeaders.Add("Referer", "https://es.wallapop.com/");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("X-AppVersion", APP_VERSION);
            client.DefaultRequestHeaders.Add("X-DeviceID", deviceId);
            client.DefaultRequestHeaders.Add("X-DeviceOS", "0");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"129\", \"Not=A?Brand\";v=\"8\", \"Chromium\";v=\"129\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        }

        public async Task<string> MakeRequestAsync(string keywords, int pagestoscrap, string? latitude, string? longitude, int? minprice, int? maxprice)
        {
            latitude = latitude ?? "41.76401";
            longitude = longitude ?? "-2.46883";
            keywords = keywords ?? "quad";
            minprice = minprice ?? 1000;
            maxprice = maxprice ?? 2000;

            List<DataLayer.Models.Wallapop.Root> anuncios = new List<DataLayer.Models.Wallapop.Root>();

            try
            {
                await client.GetAsync("https://es.wallapop.com");
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(500, 1500)));

                for (int page = 0; page < pagestoscrap; page++)
                {
                    var start = page * 40;
                    var apiUrl = $"https://api.wallapop.com/api/v3/general/search?keywords={keywords}" +
                                 $"&filters_source=search_box" +
                                 $"&latitude={latitude}" +
                                 $"&longitude={longitude}" +
                                 $"&min_sale_price={minprice}" +
                                 $"&max_sale_price={maxprice}" +
                                 $"&start={start}" +
                                 $"&show_multiple_sections=false";

                    var response = await client.GetAsync(apiUrl);

                    // Verificar el estado de la respuesta
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error en la petición: {response.StatusCode}");
                        continue;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(json);

                    // deserealizar al objeto original
                    var pageAnuncios = JsonConvert.DeserializeObject<List<ScrapperGateway.Models.Wallapop.Root>>(data["search_objects"].ToString());



                    //mapear el objeto original al grup
                    var mappedAnuncios = _mapper.Map<List<DataLayer.Models.Wallapop.Root>>(pageAnuncios);
                    var hola = mappedAnuncios;
                    anuncios.AddRange(mappedAnuncios);
          
                 

                    if (page < 1) await Task.Delay(TimeSpan.FromSeconds(1));
                }



                return JsonConvert.SerializeObject(anuncios);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la petición: {ex.Message}");
                return JsonConvert.SerializeObject(new List<DataLayer.Models.Wallapop.Root>());
            }
        }
    }
}

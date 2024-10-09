using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;



namespace DataLayer
{



    public class Flags
    {
        public bool pending { get; set; }
        public bool sold { get; set; }
        public bool reserved { get; set; }
        public bool banned { get; set; }
        public bool expired { get; set; }
        public bool onhold { get; set; }
    }

    public class Image
    {
        public string original { get; set; }
        public string xsmall { get; set; }
        public string small { get; set; }
        public string large { get; set; }
        public string medium { get; set; }
        public string xlarge { get; set; }
        public int original_width { get; set; }
        public int original_height { get; set; }
    }

    public class Image2
    {
        public string original { get; set; }
        public string xsmall { get; set; }
        public string small { get; set; }
        public string large { get; set; }
        public string medium { get; set; }
        public string xlarge { get; set; }
        public int original_width { get; set; }
        public int original_height { get; set; }
    }

    public class Location
    {
        public string city { get; set; }
        public string postal_code { get; set; }
        public string country_code { get; set; }
    }

    public class Root
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public double distance { get; set; }
        public List<Image> images { get; set; }
        public User user { get; set; }
        public Flags flags { get; set; }
        public VisibilityFlags visibility_flags { get; set; }
        public double price { get; set; }
        public string currency { get; set; }
        public bool free_shipping { get; set; }
        public string web_slug { get; set; }
        public int category_id { get; set; }
        public Shipping shipping { get; set; }
        public bool supports_shipping { get; set; }
        public bool shipping_allowed { get; set; }
        public string seller_id { get; set; }
        public bool favorited { get; set; }
        public DateTime creation_date { get; set; }
        public DateTime modification_date { get; set; }
        public Location location { get; set; }
        public TypeAttributes type_attributes { get; set; }
        public List<int> taxonomy { get; set; }
        public object discount { get; set; }
        public bool is_refurbished { get; set; }
    }

    public class Shipping
    {
        public bool item_is_shippable { get; set; }
        public bool user_allows_shipping { get; set; }
        public object cost_configuration_id { get; set; }
    }

    public class TypeAttributes
    {
    }

    public class User
    {
        public string id { get; set; }
        public Image image { get; set; }
        public bool online { get; set; }
        public string kind { get; set; }
        public string micro_name { get; set; }
    }

    public class VisibilityFlags
    {
        public bool bumped { get; set; }
        public bool highlighted { get; set; }
        public bool urgent { get; set; }
        public bool country_bumped { get; set; }
        public bool boosted { get; set; }
    }


    public class Web3Data : IWeb3Data
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string deviceId;
        private readonly string mpid;
        private const string APP_VERSION = "83070";

        public Web3Data()
        {
            deviceId = Guid.NewGuid().ToString();
            mpid = GenerateMPID();
            SetupHttpClient();
        }

        private string GenerateMPID()
        {
            // Genera un MPID similar al formato observado (número de 19 dígitos)
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

        public async Task<string> MakeRequestAsync(string keywords, string? latitude, string? longitude, int? minprice, int? maxprice)
        {
            latitude = latitude ?? "41.76401";
            longitude = longitude ?? "-2.46883";
            keywords = keywords ?? "quad";
            minprice = minprice ?? 1000;
            maxprice = maxprice ?? 2000;

            List<Root> anuncios = new List<Root>();

            try
            {
                // Primero hacemos una petición a la página principal para establecer cookies
                await client.GetAsync("https://es.wallapop.com");

                // Pequeña pausa para simular comportamiento humano
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(500, 1500)));

                for (int page = 0; page < 2; page++)
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

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error en la petición: {response.StatusCode}");
                        continue;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(json);

                    foreach (var item in data["search_objects"])
                    {
                        var anuncio = new Root
                        {
                            id = item["id"].ToString(),
                            title = item["title"].ToString(),
                            description = item["description"].ToString(),
                            distance = (double)item["distance"],
                            price = (double)item["price"],
                            currency = item["currency"].ToString(),
                            free_shipping = (bool)item["free_shipping"],
                            web_slug = item["web_slug"].ToString(),
                            category_id = (int)item["category_id"],
                            seller_id = item["seller_id"].ToString(),
                            creation_date = (DateTime)item["creation_date"],
                            modification_date = (DateTime)item["modification_date"],
                            location = new Location
                            {
                                city = item["location"]["city"].ToString(),
                                postal_code = item["location"]["postal_code"].ToString(),
                                country_code = item["location"]["country_code"].ToString(),
                            },
                            images = new List<Image>()
                        };

                        foreach (var image in item["images"])
                        {
                            anuncio.images.Add(new Image
                            {
                                original = image["original"]?.ToString(),
                                xsmall = image["xsmall"]?.ToString(),
                                small = image["small"]?.ToString(),
                                large = image["large"]?.ToString(),
                                medium = image["medium"]?.ToString(),
                                xlarge = image["xlarge"]?.ToString(),
                                original_width = (int)image["original_width"],
                                original_height = (int)image["original_height"]
                            });
                        }

                        anuncios.Add(anuncio);
                    }

                    // Pequeña pausa entre páginas
                    if (page < 1) await Task.Delay(TimeSpan.FromSeconds(1));
                }

                // Mantener el mismo formato de salida por consola
                Console.WriteLine($"Total de anuncios obtenidos: {anuncios.Count}");
                foreach (var anuncio in anuncios)
                {
                    Console.WriteLine("Título: " + anuncio.title);
                    Console.WriteLine("Descripción: " + anuncio.description);
                    Console.WriteLine("Precio: " + anuncio.price + " " + anuncio.currency);
                    Console.WriteLine("Ciudad: " + anuncio.location.city);
                    Console.WriteLine("-----------------------------------");
                }

                return JsonConvert.SerializeObject(anuncios);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la petición: {ex.Message}");
                return JsonConvert.SerializeObject(new List<Root>());
            }
        }
    }
}
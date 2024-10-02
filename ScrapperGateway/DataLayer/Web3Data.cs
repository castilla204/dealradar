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
        public async Task<string> MakeRequestAsync(string keywords, string? latitude, string? longitude, int? minprice, int? maxprice)
        {
            string apiUrl = "https://api.wallapop.com/api/v3/general/search";
             latitude =  latitude ?? "41.76401";
             longitude = longitude ?? "-2.46883";
             keywords = keywords ?? "quad";
             minprice = minprice ?? 1000;
             maxprice = maxprice ?? 2000;

            // Lista para almacenar los anuncios
            List<Root> anuncios = new List<Root>();

            for (int page = 0; page < 2; page++) // Número de páginas a recorrer
            {
                var start = page * 40;
                var response = await client.GetAsync($"{apiUrl}?keywords={keywords}&filters_source=search_box&latitude={latitude}&longitude={longitude}&min_sale_price={minprice}&max_sale_price={maxprice}&start={start}");
                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                foreach (var item in data["search_objects"])
                {
                    // Crear un nuevo anuncio y llenarlo con datos
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

                    // Añadir imágenes
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

                    // Añadir el anuncio a la lista
                    anuncios.Add(anuncio);
                }
            }

            // Imprimir el total de anuncios obtenidos
            Console.WriteLine($"Total de anuncios obtenidos: {anuncios.Count}");

            // Ejemplo: Imprimir detalles de cada anuncio
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
    }
    }

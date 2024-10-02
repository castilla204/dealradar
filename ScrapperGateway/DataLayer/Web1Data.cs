using System.Net.Http.Headers;
using System.Text;

namespace DataLayer
{
    public class Web1Data : IWeb1Data
    {
        private static readonly HttpClient client = new HttpClient();
        public async Task<string> MakeRequestAsync(int brandid, int modelid)
        {
            // URL de la API
            var url = "https://web.gw.coches.net/search/listing";

            // Configuración de las cabeceras
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-schibsted-tenant", "coches");

            // Cuerpo de la petición
            var requestBody = new
            {
                pagination = new { page = 1, size = 150 },
                sort = new { order = "desc", term = "relevance" },
                filters = new
                {
                    price = new { from = (decimal?)null, to = (decimal?)null },
                    priceRank = new object[] { },
                    batteryCapacity = new { from = (decimal?)null, to = (decimal?)null },
                    bodyTypeIds = new int[] { },
                    categories = new { category1Ids = new int[] { 2500 } },
                    chargingTimeFastMode = new { from = (decimal?)null, to = (decimal?)null },
                    chargingTimeStandardMode = new { from = (decimal?)null, to = (decimal?)null },
                    contractId = 0,
                    drivenWheelsIds = new int[] { },
                    electricAutonomy = new { from = (decimal?)null },
                    entry = (string)null,
                    environmentalLabels = new object[] { },
                    equipments = new object[] { },
                    fuelTypeIds = new int[] { },
                    hasPhoto = (bool?)null,
                    hasStock = (bool?)null,
                    hasWarranty = (bool?)null,
                    hp = new { from = (decimal?)null, to = (decimal?)null },
                    isCertified = false,
                    km = new { from = (decimal?)null, to = (decimal?)null },
                    luggageCapacity = new { from = (decimal?)null, to = (decimal?)null },
                    maxTerms = (int?)null,
                    onlyPeninsula = false,
                    offerTypeIds = new[] { 5, 2, 4, 0, 3 },
                    provinceIds = new int[] { },
                    rating = new { from = (decimal?)null, to = (decimal?)null },
                    searchText = (string)null,
                    sellerTypeId = 0,
                    transmissionTypeId = 0,
                    vehicles = new[]
                    {
                        new { make = (string)null, makeId = brandid, model = (string)null, modelId = modelid }
                    },
                    year = new { from = (int?)null, to = (int?)null }
                }
            };

            // Serializar el cuerpo a JSON
            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

            // Crear el contenido de la solicitud
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                // Realizar la petición POST
                var response = await client.PostAsync(url, content);

                // Comprobar si la respuesta fue exitosa
                response.EnsureSuccessStatusCode();

                // Leer la respuesta como una cadena
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error en la petición: {e.Message}");
                return null;
            }
        }
    }
}

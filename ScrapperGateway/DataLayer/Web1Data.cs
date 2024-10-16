using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoMapper;
using ScrapperGateway.Models.CochesNet;
using DataLayer.Models.Wallapop;

namespace DataLayer
{
    public class Web1Data : IWeb1Data
    {
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClientHandler _handler;
        private readonly Random _random = new Random();
        private readonly IMapper _mapper;

        public Web1Data(IMapper mapper)
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };
            _client = new HttpClient(_handler);
            _mapper = mapper;
        }

        private string GenerateSessionId()
        {
            return Guid.NewGuid().ToString();
        }

        private async Task GetInitialCookies()
        {
            await _client.GetAsync("https://www.coches.net");
        }

        public async Task<string> MakeRequestAsync(int brandId, int modelId)
        {
            try
            {
                await GetInitialCookies();

                var request = new HttpRequestMessage(HttpMethod.Post, "https://web.gw.coches.net/search/listing");

                // Headers esenciales y que deben variar
                request.Headers.Add("accept", "application/json, text/plain, */*");
                request.Headers.Add("accept-language", "es-ES,es;q=0.9");
                request.Headers.Add("cache-control", "no-cache");
                request.Headers.Add("dnt", "1");
                request.Headers.Add("origin", "https://www.coches.net");
                request.Headers.Add("pragma", "no-cache");
                request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");

                // Headers que necesitan variar
                var sessionId = GenerateSessionId();
                var refererUrl = $"https://www.coches.net/segunda-mano/?MakeIds%5B0%5D={brandId}&ModelIds%5B0%5D={modelId}";

                request.Headers.Add("referer", refererUrl);
                request.Headers.Add("x-adevinta-page-url", refererUrl);
                request.Headers.Add("x-adevinta-referer", refererUrl);
                request.Headers.Add("x-adevinta-session-id", sessionId);
                request.Headers.Add("x-schibsted-tenant", "coches");

                // Payload exacto con solo las variaciones necesarias
                var payload = $@"{{
                    ""pagination"":{{
                        ""page"":1,
                        ""size"":30
                    }},
                    ""sort"":{{
                        ""order"":""desc"",
                        ""term"":""relevance""
                    }},
                    ""filters"":{{
                        ""price"":{{""from"":null,""to"":null}},
                        ""priceRank"":[],
                        ""batteryCapacity"":{{""from"":null,""to"":null}},
                        ""bodyTypeIds"":[],
                        ""categories"":{{""category1Ids"":[2500]}},
                        ""chargingTimeFastMode"":{{""from"":null,""to"":null}},
                        ""chargingTimeStandardMode"":{{""from"":null,""to"":null}},
                        ""contractId"":0,
                        ""drivenWheelsIds"":[],
                        ""electricAutonomy"":{{""from"":null,""to"":null}},
                        ""entry"":null,
                        ""environmentalLabels"":[],
                        ""equipments"":[],
                        ""fuelTypeIds"":[],
                        ""hasPhoto"":null,
                        ""hasStock"":null,
                        ""hasWarranty"":null,
                        ""hp"":{{""from"":null,""to"":null}},
                        ""isCertified"":false,
                        ""km"":{{""from"":null,""to"":null}},
                        ""luggageCapacity"":{{""from"":null,""to"":null}},
                        ""maxTerms"":null,
                        ""onlyPeninsula"":false,
                        ""offerTypeIds"":[5,2,4,0,3],
                        ""provinceIds"":[],
                        ""rating"":{{""from"":null,""to"":null}},
                        ""searchText"":null,
                        ""sellerTypeId"":0,
                        ""transmissionTypeId"":0,
                        ""vehicles"":[{{""makeId"":{brandId},""modelId"":{modelId}}}],
                        ""year"":{{""from"":null,""to"":null}}
                    }}
                }}";

                request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(jsonResponse);
                var cochesNetList = jsonObject["items"].ToObject<List<ScrapperGateway.Models.CochesNet.Root>>();

                var wallapopList = _mapper.Map<List<DataLayer.Models.Wallapop.Root>>(cochesNetList);

                // Serializar la lista de Wallapop a JSON
                return JsonConvert.SerializeObject(wallapopList, Formatting.Indented);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error al realizar la petición a Coches.net: {ex.Message}", ex);
            }
        }
    }
}
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using AutoMapper;
using ScrapperGateway.Models.Vinted;
using DataLayer.Models.Wallapop;
using System.Linq;

namespace DataLayer
{
    public class Web2Data : IWeb2Data
    {
        private readonly HttpClient _client;
        private CookieContainer _cookieContainer;
        private HttpClientHandler _handler;
        private readonly IMapper _mapper;

        public Web2Data(IMapper mapper)
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };
            _client = new HttpClient(_handler);
            _client.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
            _client.DefaultRequestHeaders.Add("accept-language", "es");
            _client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
            _client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"129\", \"Not=A?Brand\";v=\"8\", \"Chromium\";v=\"129\"");
            _client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            _mapper = mapper;
        }

        public async Task<string> MakeRequestAsync(string searchKey)
        {
            try
            {
                await _client.GetAsync("https://www.vinted.es");
                var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                var url = $"https://www.vinted.es/api/v2/catalog/items?page=1&per_page=96&time={timestamp}&search_text={Uri.EscapeDataString(searchKey)}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("sec-fetch-dest", "empty");
                request.Headers.Add("sec-fetch-mode", "cors");
                request.Headers.Add("sec-fetch-site", "same-origin");
                request.Headers.Add("referer", $"https://www.vinted.es/catalog?search_text={Uri.EscapeDataString(searchKey)}");
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        args.ErrorContext.Handled = true;
                    },
                    NullValueHandling = NullValueHandling.Ignore
                };

                var vintedResponse = JsonConvert.DeserializeObject<VintedResponse>(json, settings);

                if (vintedResponse == null || vintedResponse.items == null || !vintedResponse.items.Any())
                {
                    throw new Exception("No se pudieron obtener los items de Vinted o la respuesta está vacía.");
                }

                var wallapopItems = _mapper.Map<List<DataLayer.Models.Wallapop.Root>>(vintedResponse.items);

                if (wallapopItems == null || !wallapopItems.Any())
                {
                    throw new Exception("El mapeo no produjo ningún resultado.");
                }

                return JsonConvert.SerializeObject(wallapopItems, settings);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error al realizar la petición a Vinted: {ex.Message}", ex);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new Exception($"Error durante el mapeo de Vinted a Wallapop: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado durante el procesamiento de los items de Vinted: {ex.Message}", ex);
            }
        }
    }

    public class VintedResponse
    {
        public List<ScrapperGateway.Models.Vinted.Root> items { get; set; }
    }
}
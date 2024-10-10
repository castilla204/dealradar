using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json; // Para parsear JSON
using System.Collections.Generic;
using System.Net; // Para manejar las cookies

namespace DataLayer
{
    public class Web2Data: IWeb2Data
    {
        private readonly HttpClient _client;
        private CookieContainer _cookieContainer;
        private HttpClientHandler _handler;

        public Web2Data()
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };
            _client = new HttpClient(_handler);
            SetupDefaultHeaders();
        }

        private void SetupDefaultHeaders()
        {
            _client.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
            _client.DefaultRequestHeaders.Add("accept-language", "es");
            _client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
            _client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"129\", \"Not=A?Brand\";v=\"8\", \"Chromium\";v=\"129\"");
            _client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        }

        private async Task GetInitialCookies()
        {
            // Hacer una petición inicial a la página principal para obtener las cookies
            await _client.GetAsync("https://www.vinted.es");
        }

        public async Task<string> MakeRequestAsync(string searchKey)
        {
            try
            {
                // Obtener cookies iniciales
                await GetInitialCookies();

                // Construir la URL con los parámetros
                var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                var url = $"https://www.vinted.es/api/v2/catalog/items?page=1&per_page=96&time={timestamp}&search_text={Uri.EscapeDataString(searchKey)}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Añadir headers específicos de la petición
                request.Headers.Add("sec-fetch-dest", "empty");
                request.Headers.Add("sec-fetch-mode", "cors");
                request.Headers.Add("sec-fetch-site", "same-origin");
                request.Headers.Add("referer", $"https://www.vinted.es/catalog?search_text={Uri.EscapeDataString(searchKey)}");

                // Realizar la petición
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Leer y devolver el contenido
                var json = await response.Content.ReadAsStringAsync();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error al realizar la petición a Vinted: {ex.Message}", ex);
            }
        }
    }
}
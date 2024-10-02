using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json; // Para parsear JSON
using System.Collections.Generic; // Para manejar las cookies

namespace DataLayer
{
    public class Web2Data : IWeb2Data
    {
        private static readonly HttpClient client = new HttpClient();

        // Clase para representar una cookie
        public class CookieData
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Domain { get; set; }
            public string Path { get; set; }
        }

        // Método para hacer la solicitud
        public async Task<string> MakeRequestAsync(string searchKey)
        {
            // URL de la API de Vinted con los parámetros de búsqueda
            var url = $"https://www.vinted.es/api/v2/catalog/items?page=1&per_page=96&search_text={searchKey}&catalog_ids=&size_ids=&brand_ids=&status_ids=&color_ids=&material_ids=";

            // Obtener cookies del endpoint localhost:5000/cookies
            var cookies = await FetchCookiesAsync("http://localhost:5000/cookies");

            // Crear un HttpClientHandler para manejar las cookies
            var handler = new HttpClientHandler();
            var cookieContainer = new System.Net.CookieContainer();

            // Agregar las cookies al contenedor de cookies
            foreach (var cookie in cookies)
            {
                cookieContainer.Add(new Uri("https://www.vinted.es"), new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
            }

            handler.CookieContainer = cookieContainer;

            // Crear la instancia de HttpClient usando el handler
            var client = new HttpClient(handler);

            // Añadir el encabezado de la solicitud
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36");

            // Realizar la solicitud y obtener la respuesta
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return json;
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }

        // Método para obtener las cookies desde el endpoint
        private async Task<List<CookieData>> FetchCookiesAsync(string cookieUrl)
        {
            var response = await client.GetAsync(cookieUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var cookies = JsonConvert.DeserializeObject<List<CookieData>>(json);
                return cookies;
            }
            else
            {
                throw new Exception($"Error al obtener las cookies: {response.StatusCode}");
            }
        }
    }
}

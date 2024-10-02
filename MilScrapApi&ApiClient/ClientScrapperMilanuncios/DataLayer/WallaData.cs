using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientScrapperMilanuncios.Models;
using Newtonsoft.Json;

namespace ClientScrapperMilanuncios.DataLayer
{
    public class WallaData
    {
        public async Task DisplayMessageAsync()
        {
            // URL del API al que se va a enviar la solicitud POST
            var urlToPost = "http://localhost:3000/scrape";

            // Cuerpo de la solicitud en formato JSON
   
            var bodytosendinthepost = new
            {
                searchTerms = new string[] { "Honda hornet 600" },
                pagesToScrap = 10
            };

            // Serializamos el cuerpo a JSON
            var jsonBody = JsonConvert.SerializeObject(bodytosendinthepost);

            using (HttpClient httpClient = new HttpClient())
            {
                // Enviar la solicitud POST
                Console.WriteLine("Escrapeando web...");
                HttpResponseMessage response = await httpClient.PostAsync(
                    urlToPost,
                    new StringContent(jsonBody, Encoding.UTF8, "application/json") // Aquí se define el Content-Type
                );

                // Verificamos si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leemos la respuesta
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Parseamos el JSON de la respuesta a una lista de objetos Root
                    List<Root> adList = JsonConvert.DeserializeObject<List<Root>>(responseBody);

              
             
                }

        
                else
                {
                    // Si la respuesta no fue exitosa, mostramos el error
                    Console.WriteLine("Error al hacer la solicitud: " + response.StatusCode);
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Detalles: " + errorResponse);
                }
            }
        }
    }
}

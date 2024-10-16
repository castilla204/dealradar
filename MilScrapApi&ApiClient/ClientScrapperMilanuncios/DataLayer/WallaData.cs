using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientScrapperMilanuncios.Models;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace ClientScrapperMilanuncios.DataLayer
{
    public class WallaData
    {
        private readonly IMongoCollection<Root> _rootAds;

        public WallaData(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("grup");
            _rootAds = database.GetCollection<Root>("RootAds");
        }

        private async Task<bool> AdExistsAsync(string id)
        {
            return await _rootAds.Find(ad => ad.id == id).AnyAsync();
        }

        private async Task InsertAdAsync(Root ad)
        {
            await _rootAds.InsertOneAsync(ad);
        }

        public async Task DisplayMessageAsync()
        {
            var urlToPost = "http://localhost:8000/scraping";
            var bodytosendinthepost = new
            {
                searchTerms = new string[] { "ferrari" },
                pagesToScrap = 1
            };
            var jsonBody = JsonConvert.SerializeObject(bodytosendinthepost);

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
                Console.WriteLine("Escrapeando web...");
                HttpResponseMessage response = await httpClient.PostAsync(
                    urlToPost,
                    new StringContent(jsonBody, Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    List<Root> adList = JsonConvert.DeserializeObject<List<Root>>(responseBody);

                    int newAdsCount = 0;
                    foreach (var rootAd in adList)
                    {
                        if (!await AdExistsAsync(rootAd.id))
                        {
                            await InsertAdAsync(rootAd);
                            newAdsCount++;
                            Console.WriteLine($"Nuevo anuncio guardado - ID: {rootAd.id}, Título: {rootAd.title}");
                        }
                        else
                        {
                            Console.WriteLine($"Anuncio ya existe - ID: {rootAd.id}, Título: {rootAd.title}");
                        }
                    }
                    Console.WriteLine($"Se han guardado {newAdsCount} nuevos anuncios.");
                }
                else
                {
                    Console.WriteLine("Error al hacer la solicitud: " + response.StatusCode);
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Detalles: " + errorResponse);
                }
            }
        }
    }
}
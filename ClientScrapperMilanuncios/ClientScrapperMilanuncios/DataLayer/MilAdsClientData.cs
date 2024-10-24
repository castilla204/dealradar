using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientScrapperMilanuncios.Models;
using System.Timers; 

using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ClientScrapperMilanuncios.DataLayer
{
    public class MilAdsClientData
    {
        private readonly IMongoCollection<Root> _rootAds;
        private readonly IMongoCollection<SearchLog> _searchLogs;
        private static System.Timers.Timer _cleanupTimer;

        public MilAdsClientData(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("grup");
            _rootAds = database.GetCollection<Root>("RootAds");

            var searchLogDb = mongoClient.GetDatabase("grup"); 
            _searchLogs = searchLogDb.GetCollection<SearchLog>("SearchLogs");

            _cleanupTimer = new System.Timers.Timer(3600000); 
            _cleanupTimer.Elapsed += async (sender, e) => await CleanOldAdsAsync();
            _cleanupTimer.AutoReset = true; // Reinicia el temporizador automáticamente
            _cleanupTimer.Enabled = true; // Habilitar el temporizador
            // Llama a un método para limpiar registros de más de 72 horas
            CleanOldAdsAsync().Wait();
        }

        private async Task<bool> AdExistsAsync(string id)
        {
            return await _rootAds.Find(ad => ad.id == id).AnyAsync();
        }

        private async Task InsertAdAsync(Root ad)
        {
            await _rootAds.InsertOneAsync(ad);
        }

        private async Task<bool> SearchLoggedInLast24HoursAsync(string searchTerm)
        {
            var filter = Builders<SearchLog>.Filter.And(
                Builders<SearchLog>.Filter.Eq(log => log.SearchTerm, searchTerm),
                Builders<SearchLog>.Filter.Gte(log => log.Timestamp, DateTime.UtcNow.AddHours(-24))
            );
            return await _searchLogs.Find(filter).AnyAsync();
        }

        private async Task LogSearchAsync(string searchTerm)
        {
            var log = new SearchLog
            {
                SearchTerm = searchTerm,
                Timestamp = DateTime.UtcNow
            };
            await _searchLogs.InsertOneAsync(log);
        }

        private async Task CleanOldAdsAsync()
        {
            var filter = Builders<Root>.Filter.Lt(ad => ad.ScrappedDate, DateTime.UtcNow.AddHours(-72));
            await _rootAds.DeleteManyAsync(filter);
        }

        public async Task<string> DisplayMessageAsync(List<string> searchTerms, int pagesToScrape)
        {
            var urlToPost = "http://localhost:8000/scraping";
            var bodytosendinthepost = new
            {
                searchTerms = searchTerms,
                pagesToScrap = pagesToScrape
            };
            var jsonBody = JsonConvert.SerializeObject(bodytosendinthepost);

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
                Console.WriteLine("Escrapeando web...");

                List<Root> adList = new List<Root>();
                foreach (var term in searchTerms)
                {
                    if (!await SearchLoggedInLast24HoursAsync(term))
                    {
                        HttpResponseMessage response = await httpClient.PostAsync(
                            urlToPost,
                            new StringContent(jsonBody, Encoding.UTF8, "application/json")
                        );

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            adList.AddRange(JsonConvert.DeserializeObject<List<Root>>(responseBody));

                            await LogSearchAsync(term); // Registrar búsqueda
                        }
                        else
                        {
                            Console.WriteLine("Error al hacer la solicitud: " + response.StatusCode);
                            string errorResponse = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("Detalles: " + errorResponse);
                            return "error";
                        }
                    }
                    else
                    {
                        Console.WriteLine($"La búsqueda '{term}' ya se realizó en las últimas 24 horas.");
                    }
                }

                int newAdsCount = 0;
                foreach (var rootAd in adList)
                {
                    if (!await AdExistsAsync(rootAd.id))
                    {
                        // Establece la fecha y hora actual en ScrappedDate
                        rootAd.ScrappedDate = DateTime.UtcNow;

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
                return JsonConvert.SerializeObject(adList);
            }
        }
    }

    // Clase para almacenar los registros de búsqueda
    public class SearchLog
    {
        public ObjectId Id { get; set; }
        public string SearchTerm { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // Clase Root que representa un anuncio
 
}

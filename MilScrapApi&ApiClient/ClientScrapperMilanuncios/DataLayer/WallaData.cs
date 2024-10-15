using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientScrapperMilanuncios.Models;
using Newtonsoft.Json;
using AutoMapper;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ClientScrapperMilanuncios.DataLayer
{
    public class WallaData
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<GrupAd> _grupAds;

        public WallaData(IMapper mapper, IMongoClient mongoClient)
        {
            _mapper = mapper;
            var database = mongoClient.GetDatabase("grup");
            _grupAds = database.GetCollection<GrupAd>("GrupAds");
        }

        private async Task<bool> AdExistsAsync(string id)
        {
            return await _grupAds.Find(ad => ad.id == id).AnyAsync();
        }

        private async Task InsertAdAsync(GrupAd ad)
        {
            await _grupAds.InsertOneAsync(ad);
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
                // Establecer timeout ilimitado
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
                    List<GrupAd> grupAdList = _mapper.Map<List<GrupAd>>(adList);

                    int newAdsCount = 0;
                    foreach (var grupAd in grupAdList)
                    {
                        if (!await AdExistsAsync(grupAd.id))
                        {
                            await InsertAdAsync(grupAd);
                            newAdsCount++;
                            Console.WriteLine($"Nuevo anuncio guardado - ID: {grupAd.id}, Título: {grupAd.title}");
                        }
                        else
                        {
                            Console.WriteLine($"Anuncio ya existe - ID: {grupAd.id}, Título: {grupAd.title}");
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
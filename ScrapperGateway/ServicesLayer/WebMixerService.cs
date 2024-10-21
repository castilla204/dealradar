using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataLayer;
using DataLayer.Models.Wallapop;
using System.Text.Json;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Models;
using OpenAI;

namespace ServicesLayer
{
    public class WebMixerService : IWebMixerService
    {
        private readonly IWeb1Data _web1Data;
        private readonly IWeb2Data _web2Data;
        private readonly IWeb3Data _web3Data;
        private readonly IWeb4Data _web4Data;
        private readonly IOpenAIClient _openAIClient;
        private readonly IMapper _mapper;

        public WebMixerService(
            IWeb1Data web1Data,
            IWeb2Data web2Data,
            IWeb3Data web3Data,
            IWeb4Data web4Data,
            IOpenAIClient openAIClient,
            IMapper mapper)
        {
            _web1Data = web1Data;
            _web2Data = web2Data;
            _web3Data = web3Data;
            _web4Data = web4Data;
            _openAIClient = openAIClient;
            _mapper = mapper;
        }

        public async Task<string> GetAllAds(
            string keywords,
            string? searchDescription,
            int pagestoscrape,
            string? latitude,
            string? longitude,
            int? minprice,
            int? maxprice,
            int? brandId,
            int? modelId)
        {
            var allAdsList = new List<Root>();

            // Centraliza las llamadas a las APIs
            allAdsList.AddRange(await FetchAdsFromWeb1(brandId, modelId));
            allAdsList.AddRange(await FetchAdsFromWeb2(keywords));
            allAdsList.AddRange(await FetchAdsFromWeb3(keywords, pagestoscrape, latitude, longitude, minprice, maxprice));
            allAdsList.AddRange(await FetchAdsFromWeb4(keywords));

            // Filtrar y rankear los anuncios por título, precio y descripción
            var top10Ads = await FilterAndRankAdsByTextualData(allAdsList, searchDescription);

            // Analizar las imágenes de los 10 mejores anuncios
            var finalResults = await AnalyzeImagesForTop10Ads(top10Ads);

            // Convertir los resultados a formato JSON
            var resultJson = JsonSerializer.Serialize(finalResults);
            return resultJson;
        }

        private async Task<List<Root>> FetchAdsFromWeb1(int? brandId, int? modelId)
        {
            var ads = new List<Root>();
            if (brandId != null && modelId != null)
            {
                try
                {
                    var response = await _web1Data.MakeRequestAsync(brandId.Value, modelId.Value);
                    ads = JsonSerializer.Deserialize<List<Root>>(response);
                }
                catch (Exception ex)
                {
                    // Manejo de errores
                    Console.WriteLine($"Error fetching ads from Web1: {ex.Message}");
                }
            }
            return ads;
        }

        private async Task<List<Root>> FetchAdsFromWeb2(string keywords)
        {
            var ads = new List<Root>();
            try
            {
                var response = await _web2Data.MakeRequestAsync(keywords);
                ads = JsonSerializer.Deserialize<List<Root>>(response);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error fetching ads from Web2: {ex.Message}");
            }
            return ads;
        }

        private async Task<List<Root>> FetchAdsFromWeb3(string keywords, int pagestoscrape, string? latitude, string? longitude, int? minprice, int? maxprice)
        {
            var ads = new List<Root>();
            try
            {
                var response = await _web3Data.MakeRequestAsync(keywords, pagestoscrape, latitude, longitude, minprice, maxprice);
                ads = JsonSerializer.Deserialize<List<Root>>(response);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error fetching ads from Web3: {ex.Message}");
            }
            return ads;
        }

        private async Task<List<Root>> FetchAdsFromWeb4(string keywords)
        {
            var ads = new List<Root>();
            try
            {
                var response = await _web4Data.MakeRequestAsync(keywords);
                ads = JsonSerializer.Deserialize<List<Root>>(response);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error fetching ads from Web4: {ex.Message}");
            }
            return ads;
        }

        private async Task<List<AdWithDetails>> FilterAndRankAdsByTextualData(List<Root> ads, string searchDescription)
        {
            var adsWithDetails = new List<AdWithDetails>();

            foreach (var ad in ads)
            {
                var adWithDetails = new AdWithDetails
                {
                    Id = ad.id,
                    Price = ad.price.cashPrice.value,
                    Title = ad.title,
                    Description = ad.description,
                    Images = ad.images
                };

                // Utilizar el modelo GPT-3.5 para analizar cada anuncio y obtener la puntuación de relevancia
                var adScore = await AnalyzeAdRelevance(adWithDetails, searchDescription);
                adWithDetails.AdScore = adScore;

                adsWithDetails.Add(adWithDetails);
            }

            // Ordenar los anuncios por puntuación de relevancia de mayor a menor
            adsWithDetails = adsWithDetails.OrderByDescending(a => a.AdScore).ToList();

            // Seleccionar los 10 mejores anuncios
            return adsWithDetails.Take(10).ToList();
        }

        private async Task<List<FinalAdResult>> AnalyzeImagesForTop10Ads(List<AdWithDetails> top10Ads)
        {
            var finalResults = new List<FinalAdResult>();

            foreach (var ad in top10Ads)
            {
                // Utilizar el modelo GPT-4 para analizar las imágenes del anuncio
                var (goodThings, badThings, finalScore) = await AnalyzeAdImages(ad.Id, ad.Images);

                // Crear el resultado final del anuncio
                var finalResult = new FinalAdResult
                {
                    Id = ad.Id,
                    Price = ad.Price,
                    Title = ad.Title,
                    Description = ad.Description,
                    AdScore = ad.AdScore,
                    FinalScore = finalScore,
                    GoodThings = goodThings,
                    BadThings = badThings
                };

                finalResults.Add(finalResult);
            }

            // Ordenar los anuncios por puntuación final de mayor a menor
            finalResults = finalResults.OrderByDescending(r => r.FinalScore).ToList();

            // Seleccionar los 5 mejores anuncios
            return finalResults.Take(5).ToList();
        }

        private async Task<int> AnalyzeAdRelevance(AdWithDetails ad, string searchDescription)
        {
            var prompt = $"Analyze the relevance of the following ad with respect to the search description: {searchDescription}\n\nTitle: {ad.Title}\nDescription: {ad.Description}\nPrice: {ad.Price}";

            var completion = await _openAIClient.Completions.CreateCompletionAsync(new CompletionCreateRequest
            {
                Prompt = prompt,
                MaxTokens = 1,
                NucleusSamplingFactor = 0.9f,
                Temperature = 0.5f,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            });

            return int.Parse(completion.Choices.First().Text.Trim());
        }

        private async Task<(string, string, int)> AnalyzeAdImages(string adId, List<string> images)
        {
            var prompt = $"Analyze the images for the ad with ID {adId} and provide a list of the top 3 positive and negative aspects of the product based on the images.";

            var completion = await _openAIClient.Completions.CreateCompletionAsync(new CompletionCreateRequest
            {
                Prompt = prompt,
                MaxTokens = 500,
                NucleusSamplingFactor = 0.9f,
                Temperature = 0.5f,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            });

            // Procesar los resultados de GPT-4
            var result = completion.Choices.First().Text.Trim();
            var resultParts = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (resultParts.Length >= 3)
            {
                var goodThings = resultParts[0];
                var badThings = resultParts[1];
                var finalScoreStr = resultParts[2];
                var finalScore = int.Parse(finalScoreStr);

                return (goodThings, badThings, finalScore);
            }
            else
            {
                return ("", "", 0);
            }
        }

        public class AdWithDetails
        {
            public string Id { get; set; }
            public int Price { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int AdScore { get; set; }
            public List<string> Images { get; set; }
        }

        public class FinalAdResult
        {
            public string Id { get; set; }
            public int Price { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int AdScore { get; set; }
            public int FinalScore { get; set; }
            public string GoodThings { get; set; }
            public string BadThings { get; set; }
        }
    }
}

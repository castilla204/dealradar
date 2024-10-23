using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using DataLayer;
using DataLayer.Models.Wallapop;
using Microsoft.Extensions.Configuration;
using ServicesLayer;

// Clase para almacenar el formato liviano de los anuncios
public class AdLight
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
}

public class WebMixerService : IWebMixerService
{
    private readonly IWeb1Data _web1Data;
    private readonly IWeb2Data _web2Data;
    private readonly IWeb3Data _web3Data;
    private readonly IWeb4Data _web4Data;
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient;
    private readonly string _openAiApiKey;

    public WebMixerService(
        IWeb1Data web1Data,
        IWeb2Data web2Data,
        IWeb3Data web3Data,
        IWeb4Data web4Data,
        IMapper mapper,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _web1Data = web1Data;
        _web2Data = web2Data;
        _web3Data = web3Data;
        _web4Data = web4Data;
        _mapper = mapper;
        _httpClient = httpClient;

        _openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("API Key is missing in configuration.");
    }

    public async Task<List<AdLight>> AnalyzeAds(
        string keywords,
        string userSearch,
        int pagesToScrape,
        string? latitude,
        string? longitude,
        int? minPrice,
        int? maxPrice,
        int? brandId,
        int? modelId)
    {
        var allAdsList = await GetAllAds(keywords, pagesToScrape, latitude, longitude, minPrice, maxPrice, brandId, modelId);
        var adsLight = MapAdsToLightFormat(allAdsList);
        var analyzed = await AnalyzeForDeals(adsLight);


        var batches = SplitAdsIntoBatches(analyzed.PotentialDeals);
        var scores = await GetAdScoresFromAI(batches, userSearch, (int)analyzed.MedianPrice);

        return GetBestAds(adsLight, scores);
    }

    public List<AdLight> MapAdsToLightFormat(List<Root> allAdsList)
    {
        return allAdsList.Select(ad => new AdLight
        {
            Id = ad.id,
            Title = ad.title,
            Description = ad.description,
            Price = ad.price.cashPrice.value
        }).ToList();
    }

    public List<List<AdLight>> SplitAdsIntoBatches(List<AdLight> ads, int batchSize = 20)
    {
        return ads.Select((ad, index) => new { ad, index })
                  .GroupBy(x => x.index / batchSize)
                  .Select(group => group.Select(x => x.ad).ToList())
                  .ToList();
    }

    public string GeneratePrompt(List<AdLight> adsBatch, string userSearch, int averagePrice)
    {
        var adsString = string.Join("\n", adsBatch.Select(ad =>
            $"Ad ID: {ad.Id}\nTitle: {ad.Title}\nDescription: {ad.Description}\nPrice: {ad.Price}€\n"));

        return $@"
El usuario está buscando: {userSearch}.
Evalúa los siguientes anuncios de motocicletas y clasifícalos según la intención de búsqueda del usuario. Concéntrate en los siguientes criterios:

1. Los anuncios que no sean una moto puntuarlos con 0. Fíjate solo en el título. 

2. Ten en cuenta lo que está buscando el usuario y asigna puntuaciones bajas a los anuncios que no cumplen con lo que busca.

3. Relaciona el precio y la información del título y descripción para valorar el anuncio. Si no hay información negativa sobre la moto y el precio es bajo, otorgar buena puntuación.

Aquí están los anuncios:
{adsString}
Devuelve el resultado en este formato:
Ad ID: <ad_id> - Score: <score>


";
    }

    public async Task<List<(string Id, int Score)>> GetAdScoresFromAI(List<List<AdLight>> adBatches, string userSearch, int averagePrice)
    {
        var tasks = adBatches.Select(async batch =>
        {
            var prompt = GeneratePrompt(batch, userSearch, averagePrice);
            var response = await SendOpenAIRequestAsync(prompt);
            return ParseAiResponse(response);
        });

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).ToList();
    }

    private async Task<string> SendOpenAIRequestAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 300,
            temperature = 0.5
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
        return responseObject.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }

    public List<(string Id, int Score)> ParseAiResponse(string aiResponse)
    {
        var scores = new List<(string, int)>();
        var lines = aiResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.Contains("Ad ID:") && line.Contains("Score:"))
            {
                try
                {
                    var parts = line.Split('-');
                    if (parts.Length >= 2)
                    {
                        var id = parts[0].Split(':')[1].Trim();
                        var score = int.Parse(parts[1].Split(':')[1].Trim());
                        scores.Add((id, score));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing line: {line}. Exception: {ex.Message}");
                }
            }
        }
        return scores;
    }

    public List<AdLight> GetBestAds(List<AdLight> ads, List<(string Id, int Score)> scoredAds)
    {
        var uniqueScoredAds = scoredAds
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToDictionary(x => x.Id, x => x.Score);

        return ads
            .Where(ad => uniqueScoredAds.TryGetValue(ad.Id, out var score) && score > 0)
            .OrderByDescending(ad => uniqueScoredAds[ad.Id])
            .ToList();
    }

    public async Task<List<Root>> GetAllAds(string keywords, int pagesToScrape, string? latitude, string? longitude, int? minPrice, int? maxPrice, int? brandId, int? modelId)
    {
        var fetchWeb2 = FetchAdsFromWeb2(keywords);
        var fetchWeb3 = FetchAdsFromWeb3(keywords, pagesToScrape, latitude, longitude, minPrice, maxPrice);

        var allResults = await Task.WhenAll(fetchWeb2, fetchWeb3);
        return allResults.SelectMany(x => x).ToList();
    }

    public async Task<List<Root>> FetchAdsFromWeb2(string keywords)
    {
        string jsonResponse = await _web2Data.MakeRequestAsync(keywords);
        return JsonSerializer.Deserialize<List<Root>>(jsonResponse) ?? new List<Root>();
    }

    public async Task<List<Root>> FetchAdsFromWeb3(string keywords, int pagesToScrape, string? latitude, string? longitude, int? minPrice, int? maxPrice)
    {
        string jsonResponse = await _web3Data.MakeRequestAsync(keywords, pagesToScrape, latitude, longitude, minPrice ?? 0, maxPrice ?? int.MaxValue);
        return JsonSerializer.Deserialize<List<Root>>(jsonResponse) ?? new List<Root>();
    }






    //ANALISIS OFFLINE DE ANUNCIOS
    public class AdAnalysis
    {
        public double AveragePrice { get; set; }
        public double MedianPrice { get; set; }
        public double PriceStandardDeviation { get; set; }
        public Dictionary<string, int> KeywordFrequency { get; set; }
        public List<AdLight> PotentialDeals { get; set; }
        public Dictionary<string, double> PricePercentiles { get; set; }
        public List<AdLight> OutlierDeals { get; set; }
        public Dictionary<string, List<AdLight>> PriceBrackets { get; set; }
    }
    public async Task<AdAnalysis> AnalyzeForDeals(List<AdLight> ads)
    {
        var analysis = new AdAnalysis
        {
            KeywordFrequency = CalculateKeywordFrequency(ads),
            PricePercentiles = CalculatePricePercentiles(ads),
            PriceBrackets = CreatePriceBrackets(ads)
        };

        // Calcular estadísticas básicas de precios
        var prices = ads.Select(ad => ad.Price).ToList();
        analysis.AveragePrice = prices.Average();
        analysis.MedianPrice = CalculateMedian(prices);
        analysis.PriceStandardDeviation = CalculateStandardDeviation(prices);

        // Identificar chollos potenciales basados en múltiples criterios
        analysis.PotentialDeals = IdentifyPotentialDeals(ads, analysis);
        analysis.OutlierDeals = IdentifyPriceOutliers(ads, analysis);

        return analysis;
    }

    private Dictionary<string, int> CalculateKeywordFrequency(List<AdLight> ads)
    {
        var keywords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var ad in ads)
        {
            var words = ad.Title.Split(' ')
                .Concat(ad.Description.Split(' '))
                .Select(w => w.ToLower())
                .Where(w => w.Length > 3);  // Ignorar palabras muy cortas

            foreach (var word in words)
            {
                if (!keywords.ContainsKey(word))
                    keywords[word] = 0;
                keywords[word]++;
            }
        }

        return keywords.OrderByDescending(x => x.Value)
                      .Take(50)
                      .ToDictionary(x => x.Key, x => x.Value);
    }

    private double CalculateMedian(List<int> numbers)
    {
        var sorted = numbers.OrderBy(n => n).ToList();
        int mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2.0
            : sorted[mid];
    }

    private double CalculateStandardDeviation(List<int> numbers)
    {
        double average = numbers.Average();
        double sumOfSquares = numbers.Sum(n => Math.Pow(n - average, 2));
        return Math.Sqrt(sumOfSquares / (numbers.Count - 1));
    }

    private Dictionary<string, double> CalculatePricePercentiles(List<AdLight> ads)
    {
        var prices = ads.Select(a => a.Price).OrderBy(p => p).ToList();
        return new Dictionary<string, double>
        {
            {"p10", CalculatePercentile(prices, 10)},
            {"p25", CalculatePercentile(prices, 25)},
            {"p50", CalculatePercentile(prices, 50)},
            {"p75", CalculatePercentile(prices, 75)},
            {"p90", CalculatePercentile(prices, 90)}
        };
    }

    private double CalculatePercentile(List<int> numbers, int percentile)
    {
        var sorted = numbers.OrderBy(n => n).ToList();
        int index = (int)Math.Ceiling((percentile / 100.0) * sorted.Count) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
    }

    private Dictionary<string, List<AdLight>> CreatePriceBrackets(List<AdLight> ads)
    {
        var brackets = new Dictionary<string, List<AdLight>>();
        var prices = ads.Select(a => a.Price);
        var min = prices.Min();
        var max = prices.Max();
        var range = (max - min) / 5.0;

        for (int i = 0; i < 5; i++)
        {
            var lowerBound = min + (range * i);
            var upperBound = min + (range * (i + 1));
            var bracketName = $"{lowerBound:C0}-{upperBound:C0}";

            brackets[bracketName] = ads.Where(a =>
                a.Price >= lowerBound &&
                (i == 4 ? a.Price <= upperBound : a.Price < upperBound)
            ).ToList();
        }

        return brackets;
    }

    private List<AdLight> IdentifyPotentialDeals(List<AdLight> ads, AdAnalysis analysis)
    {
        return ads.Where(ad =>
        {
            // Un anuncio se considera chollo potencial si cumple varios criterios
            bool isPriceBelowAverage = ad.Price < analysis.AveragePrice * 0.8;
            bool isPriceOutlier = ad.Price < (analysis.MedianPrice - analysis.PriceStandardDeviation);
            bool hasPositiveKeywords = ContainsPositiveKeywords(ad);
            bool isNotSuspiciouslyLow = ad.Price > analysis.AveragePrice * 0.3;

            return isPriceBelowAverage && isNotSuspiciouslyLow &&
                   (isPriceOutlier || hasPositiveKeywords);
        })
        .OrderBy(ad => ad.Price)
        .ToList();
    }

    private List<AdLight> IdentifyPriceOutliers(List<AdLight> ads, AdAnalysis analysis)
    {
        var lowerBound = analysis.MedianPrice - (2 * analysis.PriceStandardDeviation);

        return ads.Where(ad =>
            ad.Price < lowerBound &&
            ad.Price > analysis.AveragePrice * 0.3 &&  // Evitar precios sospechosamente bajos
            !ContainsNegativeKeywords(ad)
        ).ToList();
    }

    private bool ContainsPositiveKeywords(AdLight ad)
    {
        var positiveKeywords = new[] {
            "nuevo", "seminuevo", "garantía", "revisión", "mantenimiento",
            "impecable", "cuidado", "único dueño", "como nuevo"
        };

        return positiveKeywords.Any(keyword =>
            ad.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            ad.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsNegativeKeywords(AdLight ad)
    {
        var negativeKeywords = new[] {
            "averiado", "accidentado", "golpe", "roto", "despiece",
            "no funciona", "para piezas", "sin documentación"
        };

        return negativeKeywords.Any(keyword =>
            ad.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            ad.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}

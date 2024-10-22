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
        IConfiguration configuration)   // Inyectar IConfiguration
    {
        _web1Data = web1Data;
        _web2Data = web2Data;
        _web3Data = web3Data;
        _web4Data = web4Data;
        _mapper = mapper;
        _httpClient = httpClient;

        // Obtener el API key desde la configuración
        _openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("API Key is missing in configuration.");
    }

    // Método principal para obtener los anuncios, procesarlos y devolver los mejores
    public async Task<List<AdLight>> AnalyzeAds(
        string keywords,
        string userSearch,
        int pagestoscrape,
        string? latitude,
        string? longitude,
        int? minprice,
        int? maxprice,
        int? brandId,
        int? modelId)
    {
        // 1. Obtener los anuncios de las diferentes plataformas
        var allAdsList = await GetAllAds(keywords, pagestoscrape, latitude, longitude, minprice, maxprice, brandId, modelId);

        // 2. Mapear los anuncios al formato liviano
        var adsLight = MapAdsToLightFormat(allAdsList);

        // 3. Dividir los anuncios en lotes de 10
        var batches = SplitAdsIntoBatches(adsLight);

        // 4. Obtener las puntuaciones de la IA
        var scores = await GetAdScoresFromAI(batches, userSearch);

        // 5. Filtrar y obtener los mejores anuncios
        return GetBestAds(adsLight, scores);
    }

    // Método para mapear los anuncios a un formato más liviano
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

    // Método para dividir los anuncios en lotes de 10
    public List<List<AdLight>> SplitAdsIntoBatches(List<AdLight> ads, int batchSize = 10)
    {
        return ads
            .Select((ad, index) => new { ad, index })
            .GroupBy(x => x.index / batchSize)
            .Select(group => group.Select(x => x.ad).ToList())
            .ToList();
    }

    // Método para generar el prompt que se enviará a la IA
    public string GeneratePrompt(List<AdLight> adsBatch, string userSearch)
    {
        var adsString = string.Join("\n", adsBatch.Select(ad =>
            $"Ad ID: {ad.Id}\nTitle: {ad.Title}\nDescription: {ad.Description}\nPrice: {ad.Price}€\n"));

        return $@"
        User is looking for: {userSearch}.
        Evaluate the following ads based on:
        - Relevance (ads that do not match the user's query should get a score of 0).
        - Quality of the title and description (better condition equals a higher score) (40%).
        - Price (better price means higher score) (40%).
        - Fewer spelling mistakes and more recent ads are rated higher (20%).

        Please return a list of Ad IDs with a score from 1 to 100 for each.
        Please provide the result in the following format:
        Ad ID: <ad_id> - Score: <score>

        Here are the ads:

        {adsString}
        ";
    }

    // Método para obtener las puntuaciones desde la IA en paralelo
    public async Task<List<(string Id, int Score)>> GetAdScoresFromAI(List<List<AdLight>> adBatches, string userSearch)
    {
        var tasks = adBatches.Select(async batch =>
        {
            var prompt = GeneratePrompt(batch, userSearch);
            var response = await SendOpenAIRequestAsync(prompt);
            var scores = ParseAiResponse(response);
            return scores;
        });

        // Esperar que todas las tareas de puntuación finalicen
        var results = await Task.WhenAll(tasks);

        // Retornar todas las puntuaciones en una lista combinada
        return results.SelectMany(r => r).ToList();
    }

    // Método para enviar el request directamente a la API de OpenAI
    private async Task<string> SendOpenAIRequestAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
            new { role = "user", content = prompt }
        },
            max_tokens = 150,
            temperature = 0.7
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        response.EnsureSuccessStatusCode();

        try
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseJson);
            return responseObject.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        catch (Exception ex)
        {
            // Manejo de errores o logging
            throw new Exception("Error parsing OpenAI response", ex);
        }

    }

    // Método para parsear la respuesta de la IA y extraer los IDs y puntuaciones
    public List<(string Id, int Score)> ParseAiResponse(string aiResponse)
    {
        var scores = new List<(string, int)>();
        var lines = aiResponse.Split('\n');

        foreach (var line in lines)
        {
            // Verifica si la línea contiene "Ad ID:" y tiene un formato válido
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
                    // Puedes hacer un log del error o ignorar las líneas mal formateadas
                    Console.WriteLine($"Error parsing line: {line}. Exception: {ex.Message}");
                }
            }
            else
            {
                // Log o manejo si la línea no tiene el formato esperado
                Console.WriteLine($"Skipping line: {line}, does not contain expected format.");
            }
        }
        return scores;
    }


    // Método para filtrar y ordenar los anuncios según sus puntuaciones
    public List<AdLight> GetBestAds(List<AdLight> ads, List<(string Id, int Score)> scoredAds)
    {
        // Filtrar duplicados manteniendo solo la primera aparición
        var uniqueScoredAds = scoredAds
            .GroupBy(x => x.Id)
            .Select(g => g.First()) // Selecciona la primera aparición en caso de duplicados
            .ToDictionary(x => x.Id, x => x.Score);

        // Filtrar anuncios con puntuación mayor a 0 y ordenarlos por la puntuación
        var topads = ads
            .Where(ad => uniqueScoredAds.ContainsKey(ad.Id) && uniqueScoredAds[ad.Id] > 0)
            .OrderByDescending(ad => uniqueScoredAds[ad.Id])
            .ToList();

        return topads;
    }


    // Método  para obtener todos los anuncios de las 4 plataformas
    public async Task<List<Root>> GetAllAds(string keywords, int pagestoscrape, string? latitude, string? longitude, int? minprice, int? maxprice, int? brandId, int? modelId)
    {
        //var fetchWeb1 = FetchAdsFromWeb1(brandId, modelId);
        var fetchWeb2 = FetchAdsFromWeb2(keywords);
        var fetchWeb3 = FetchAdsFromWeb3(keywords, pagestoscrape, latitude, longitude, minprice, maxprice);
        //var fetchWeb4 = FetchAdsFromWeb4(keywords);

        // Ejecutar todas las llamadas en paralelo
        var allResults = await Task.WhenAll(/*fetchWeb1*/ fetchWeb2, fetchWeb3 /*fetchWeb4*/);

        // Combinar todos los resultados
        return allResults.SelectMany(x => x).ToList();
    }

    // Métodos simulados para extraer los anuncios de cada plataforma
    //public async Task<List<Root>> FetchAdsFromWeb1(int? brandId, int? modelId)
    //{
    //    if (brandId != null && modelId != null)
    //    {
    //        string jsonResponse = await _web1Data.MakeRequestAsync(brandId ?? 0, modelId ?? 0);

    //        List<Root> ads = JsonSerializer.Deserialize<List<Root>>(jsonResponse);

    //        return ads;
    //    }
    //    return new List<Root>();
    //}

    // Método para obtener anuncios de la Web2 (simulando recibir un JSON)
    public async Task<List<Root>> FetchAdsFromWeb2(string keywords)
    {
        string jsonResponse = await _web2Data.MakeRequestAsync(keywords);
        List<Root> ads = JsonSerializer.Deserialize<List<Root>>(jsonResponse);

        return ads;
    }

    // Método para obtener anuncios de la Web3 (simulando recibir un JSON)
    public async Task<List<Root>> FetchAdsFromWeb3(string keywords, int pagestoscrape, string? latitude, string? longitude, int? minprice, int? maxprice)
    {
        string jsonResponse = await _web3Data.MakeRequestAsync(keywords, pagestoscrape, latitude, longitude, minprice ?? 0, maxprice ?? 999999999);
        List<Root> ads = JsonSerializer.Deserialize<List<Root>>(jsonResponse);

        return ads;
    }

    // Método para obtener anuncios de la Web4 (simulando recibir un JSON)
    //public async Task<List<Root>> FetchAdsFromWeb4(string keywords)
    //{
    //    string jsonResponse = await _web4Data.MakeRequestAsync(keywords);
    //    List<Root> ads = JsonSerializer.Deserialize<List<Root>>(jsonResponse);

    //    return ads;
    //}
}
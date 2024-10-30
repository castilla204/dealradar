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

public class Categorys
{
    public int Id { get; set; }
    public string Name { get; set; }
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
    private readonly List<Categorys> categorias;
    private  List<AdLight> potentialDeals;
    private List<Root> allAdsList;




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
        potentialDeals = new();

        _openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("API Key is missing in configuration.");

        //categorias = new List<Categorys>
        //    {
        //    new Categorys { Id = 1, Name = "Coches" },
        //    new Categorys { Id = 2, Name = "Motos" },
        //    new Categorys { Id= 3, Name= "Inmobiliaria"},
        //    new Categorys { Id = 4, Name = "Moda" },
        //};




    }

    public async Task<List<Root>> AnalyzeAds(
        string keywords,
        string userSearch,
        int pagesToScrape,
        int? category,
        string? latitude,
        string? longitude,
        int? minPrice,
        int? maxPrice,
        int? brandId,
        int? modelId)
    {
        allAdsList = await GetAllAds(keywords, pagesToScrape, category, latitude, longitude, minPrice, maxPrice, brandId, modelId);
        var adsLight = MapAdsToLightFormat(allAdsList);
        var analyzed = await AnalyzeForDeals(adsLight);
        potentialDeals = analyzed.PotentialDeals;
        List<string> DealsIdList = potentialDeals.Select(deal => deal.Id).ToList();
        var batches = SplitAdsIntoBatches(adsLight);
        var scores = await GetAdScoresFromAI(batches, userSearch, (int)analyzed.MedianPrice, category, DealsIdList);

        var hola= GetBestAds(adsLight, scores);
        return hola;
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

    public string GeneratePrompt(List<AdLight> adsBatch, string userSearch, int averagePrice, int? category, List<String> DealIdList)
    {
        var adsString = string.Join("\n", adsBatch.Select(ad =>
            $"ID: {ad.Id}\nTítulo: {ad.Title}\nDescripción: {ad.Description}\nPrecio: {ad.Price}€\n"));

        var dealIdsString = $"IDs de ofertas potenciales: {string.Join(", ", DealIdList)}\n";

        // Base criteria that applies to all categories
        var baseCriteria = $@"
        CONTEXTO Y ESTADÍSTICAS DEL MERCADO:
        - Búsqueda del usuario: '{userSearch}'
        - Precio medio del mercado: {averagePrice}€
        - Chollos: {dealIdsString}
        - Rango de precios considerado chollo: Por debajo del {averagePrice * 0.6}€
        - Precio mínimo aceptable (para evitar estafas): {averagePrice * 0.2}€";

        // Category-specific criteria
        string categoryCriteria = category switch
        {
            1 => @"
        CRITERIOS ESPECÍFICOS PARA COCHES:
        - Valorar positivamente:
  
            * Kilometraje bajo para el año del vehículo
            * Libro de mantenimiento al día
            * ITV reciente y en vigor
            * Un solo propietario
            * Historial de servicio oficial
        - Penalizar:
            * Kilometraje excesivo (>200,000 km)
            * Ausencia de documentación importante
            * Menciones a problemas mecánicos
            * ITV caducada o próxima a caducar",

            2 => @"
        CRITERIOS ESPECÍFICOS PARA MOTOS:
        - Valorar positivamente:
            * Kilometraje bajo para el año
            * Revisiones al día
            * ITV en vigor
            * Neumáticos en buen estado
            * Guardada en garaje
        - Penalizar:
            * Kilometraje alto (>50,000 km)
            * Caídas o golpes
            * Modificaciones no homologadas
            * Problemas de motor o transmisión",

            3 => @"
        CRITERIOS ESPECÍFICOS PARA INMOBILIARIA:
        - Valorar positivamente:
            * Precio por m² inferior a la media de la zona
            * Buena ubicación mencionada
            * Reformas recientes
            * Características extra (parking, trastero, etc)
            * Orientación y luminosidad
        - Penalizar:
            * Ausencia de metros cuadrados en descripción
            * Necesidad de reforma integral
            * Problemas estructurales mencionados
            * Falta de documentación o situaciones legales complejas",

            4 => @"
        CRITERIOS ESPECÍFICOS PARA MODA:
        - Valorar positivamente:
            * Artículos nuevos con etiqueta
            * Marcas premium a precio reducido
            * Ediciones limitadas o exclusivas
            * Descripción detallada del estado
        - Penalizar:
            * Daños o defectos significativos
            * Falta de información sobre talla/medidas
            * Signos de desgaste excesivo
            * Posibles falsificaciones",

            _ => "" // Categoría no específica
        };

        return $@"Actúa como un experto analista de mercado especializado en identificar chollos y buenas ofertas en anuncios de segunda mano.

        {baseCriteria}

        {categoryCriteria}

        SEÑALES POSITIVAS GENERALES A VALORAR:
        - Palabras clave positivas: nuevo, seminuevo, garantía, revisión, mantenimiento, impecable, cuidado, único dueño, como nuevo
        - Precio por debajo del 80% de la media del mercado
        - Descripción detallada y profesional
        - Menciones de mantenimiento o cuidados
        - Garantías o posibilidad de prueba

        SEÑALES NEGATIVAS GENERALES A PENALIZAR:
        - Palabras clave negativas: averiado, accidentado, golpe, roto, despiece, no funciona, para piezas, sin documentación
        - Precio sospechosamente bajo (menos del 30% de la media)
        - Descripción vaga o incompleta
        - Señales de posible estafa o producto en mal estado

        TAREA:
        Analiza cada anuncio y asigna una puntuación del 0 al 100 basándote en los siguientes criterios:

        1. RELEVANCIA (0 o 100):
        - Si el anuncio NO corresponde a la categoría buscada, asigna 0 puntos y detén el análisis
        - Si el id del anuncio corresponde con uno de los id chollo entonces este anuncio tendra minimo un 100
        - Si corresponde, continúa con los siguientes criterios

        2. COINCIDENCIA CON BÚSQUEDA (0-20 puntos):
        - Evalúa qué tan bien coincide el artículo con lo que busca el usuario
        - Considera palabras clave específicas y variantes

        3. RELACIÓN CALIDAD-PRECIO (0-50 puntos):
        - Compara el precio con la media del mercado ({averagePrice}€)
        - Otorga máxima puntuación a precios entre 30-80% de la media con señales positivas
        - Penaliza fuertemente precios por debajo del 30% de la media
        - Valora el estado y características mencionadas según los criterios específicos de la categoría

        4. CALIDAD DE INFORMACIÓN (0-30 puntos):
        - Evalúa la completitud y claridad de la descripción
        - Busca señales positivas específicas de la categoría
        - Penaliza señales de alerta según la categoría
    
        ANUNCIOS A EVALUAR:
        {adsString}

       FORMATO DE RESPUESTA:
Responde ÚNICAMENTE con el siguiente formato para cada anuncio:
Ad ID: <id> - Score: <puntuación>
Positivos: [lista de aspectos positivos separados por coma]
Negativos: [lista de aspectos negativos separados por coma]

IMPORTANTE:
- NO incluyas explicaciones ni comentarios adicionales
- Solo números enteros del 0 al 100 para el puntaje
- Un anuncio por línea, con listas en formato indicado
- Mantén estrictamente el formato especificado";
    }

    public async Task<List<(string Id, int Score, List<string> Positives, List<string> Negatives)>> GetAdScoresFromAI(
     List<List<AdLight>> adBatches, string userSearch, int averagePrice, int? category, List<string> DealsIdList)
    {
        var tasks = adBatches.Select(async batch =>
        {
            var prompt = GeneratePrompt(batch, userSearch, averagePrice, category, DealsIdList);
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
            model = "gpt-4o-mini",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 500,
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

    public List<(string Id, int Score, List<string> Positives, List<string> Negatives)> ParseAiResponse(string aiResponse)
    {
        var results = new List<(string, int, List<string>, List<string>)>();
        var lines = aiResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string id = "";
        int score = 0;
        List<string> positives = new List<string>();
        List<string> negatives = new List<string>();

        foreach (var line in lines)
        {
            if (line.Contains("Ad ID:") && line.Contains("Score:"))
            {
                try
                {
                    var parts = line.Split('-');
                    id = parts[0].Split(':')[1].Trim();
                    score = int.Parse(parts[1].Split(':')[1].Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing line: {line}. Exception: {ex.Message}");
                }
            }
            else if (line.StartsWith("Positivos:"))
            {
                positives = line.Substring(line.IndexOf(':') + 1)
                    .Split(',')
                    .Select(p => p.Trim())
                    .ToList();
            }
            else if (line.StartsWith("Negativos:"))
            {
                negatives = line.Substring(line.IndexOf(':') + 1)
                    .Split(',')
                    .Select(n => n.Trim())
                    .ToList();

                if (!string.IsNullOrEmpty(id))
                {
                    results.Add((id, score, positives, negatives));
                    id = ""; score = 0;
                    positives = new List<string>();
                    negatives = new List<string>();
                }
            }
        }
        return results;
    }



    public List<Root> GetBestAds(List<AdLight> ads, List<(string Id, int Score, List<string> Positives, List<string> Negatives)> scoredAds)
    {
        // Crear un diccionario único que mapea el ID con la tupla completa de score, positivos y negativos.
        var scoreDictionary = scoredAds.ToDictionary(ad => ad.Id, ad => (ad.Score, ad.Positives, ad.Negatives));

        // Asignar las puntuaciones finales y detalles a los anuncios en un solo bucle
        foreach (var item in allAdsList)
        {
            if (scoreDictionary.TryGetValue(item.id, out var details))
            {
                item.finalScore = details.Score;
                item.goodThings = details.Positives;
                item.badThings = details.Negatives;
            }
            else
            {
                item.finalScore = 0;
                item.goodThings = new List<string>();
                item.badThings = new List<string>();
            }
        }

        // Ordenar la lista por finalScore en orden descendente
        return allAdsList
            .OrderByDescending(ad => ad.finalScore)
            .ToList();
    }


    public async Task<List<Root>> GetAllAds(string keywords, int pagesToScrape, int? category, string? latitude, string? longitude, int? minPrice, int? maxPrice, int? brandId, int? modelId)
    {
        try
        {
            List<Task<List<Root>>> tasks = new List<Task<List<Root>>>();

            // Solo añadir FetchAdsFromWeb2 si la categoría es 4 (moda)
            if (category == 4)
            {
                tasks.Add(FetchAdsFromWeb2(keywords));
            }

            // Añadir siempre las otras tareas

            tasks.Add(FetchAdsFromWeb3(keywords, pagesToScrape, category, latitude, longitude, minPrice, maxPrice));
            tasks.Add(FetchAdsFromWeb4(keywords, pagesToScrape, category));

            var allResults = await Task.WhenAll(tasks);
            return allResults.SelectMany(x => x).ToList();
        }catch(Exception ex)
        {
            throw ex;
        }
    }

    public async Task<List<Root>> FetchAdsFromWeb2(string keywords)
    {
        string jsonResponse = await _web2Data.MakeRequestAsync(keywords);
        return JsonSerializer.Deserialize<List<Root>>(jsonResponse) ?? new List<Root>();
    }

    public async Task<List<Root>> FetchAdsFromWeb3(string keywords, int pagesToScrape, int? category ,string? latitude, string? longitude, int? minPrice, int? maxPrice)
    {
        int categoryInt = 0;
        if (category == 1)
        {
            categoryInt = 100;
        }
        if (category == 2)
        {
            categoryInt = 14000;
        }
        if (category == 3)
        {
            categoryInt = 200;
        }
        if (category == 4)
        {
            categoryInt = 12465;
        }


        string jsonResponse = await _web3Data.MakeRequestAsync(keywords, pagesToScrape, categoryInt, latitude, longitude, minPrice ?? 0, maxPrice ?? int.MaxValue);
        return JsonSerializer.Deserialize<List<Root>>(jsonResponse) ?? new List<Root>();
    }



    public async Task<List<Root>> FetchAdsFromWeb4(string keyword, int pagesToScrape, int? category)
    {
        string categoryString = "";
        if(category == 1)
        {
            categoryString = "coches-de-segunda-mano";
        }if(category == 2)
        {
            categoryString = "motos-de-segunda-mano";
        }if(category == 3)
        {
            categoryString = "inmobiliaria";
        }if(category == 4)
        {
            categoryString = "moda-y-complementos";
        }



        var url = "https://localhost:7184/api/MilAds/scrape";
        

        // Crear el cuerpo de la solicitud JSON
        var requestBody = new
        {
            searchTerms = new List<string> { keyword },
            pagesToScrape = pagesToScrape,
            category = categoryString
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        Console.WriteLine("Realizando solicitud a la API Web4...");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response: {result}");
                    List<Root> lista = await _web4Data.GetAnunciosAsync(keyword);
                    return lista;
                
                }catch(Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("La solicitud ha tardado demasiado y ha sido cancelada.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }

        return new List<Root>();
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

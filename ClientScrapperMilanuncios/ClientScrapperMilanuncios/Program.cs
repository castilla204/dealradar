using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using MongoDB.Driver;
using ClientScrapperMilanuncios.Models;
using ClientScrapperMilanuncios.DataLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MilAds Scraper API",
        Version = "v1",
        Description = "API para el scraping de anuncios"
    });
});

// Configurar MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddScoped<MilAdsClientData>();

var app = builder.Build();

// Configure the HTTP request pipeline
// Habilitar Swagger para todos los entornos (desarrollo y producción)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MilAds Scraper API V1");
    // Configurar Swagger UI como página de inicio
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Agregar una redirección de la raíz a Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
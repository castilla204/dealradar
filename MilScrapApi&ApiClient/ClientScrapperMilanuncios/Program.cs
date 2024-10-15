using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ClientScrapperMilanuncios.DataLayer;

using MongoDB.Driver;

using ClientScrapperMilanuncios.Models.ClientScrapperMilanuncios.Models.ClientScrapperMilanuncios.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Configurar AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperClass));

// Configurar MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

// Agregar WallaData como servicio
builder.Services.AddScoped<WallaData>();

var app = builder.Build();

// Definimos una ruta de prueba
app.MapGet("/", () => "Hello World!");

// Llamamos a la función de la clase WallaData para que ejecute el scraping al arrancar
using (var scope = app.Services.CreateScope())
{
    var wallaData = scope.ServiceProvider.GetRequiredService<WallaData>();
    await wallaData.DisplayMessageAsync();
}

app.Run();
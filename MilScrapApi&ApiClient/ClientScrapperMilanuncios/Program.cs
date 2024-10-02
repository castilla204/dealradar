using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ClientScrapperMilanuncios.DataLayer; // Añadir referencia a DataLayer
using System.Threading.Tasks; // Para usar Task

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Definimos una ruta de prueba
app.MapGet("/", () => "Hello World!");

// Llamamos a la función de la clase WallaData para que ejecute el scraping al arrancar
var wallaData = new WallaData(); // Creamos una instancia de la clase WallaData
await wallaData.DisplayMessageAsync(); // Llamamos al método asincrónico

app.Run();

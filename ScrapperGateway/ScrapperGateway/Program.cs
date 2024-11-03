using AutoMapper;
using DataLayer;
using DataLayer.Mappers;
using DataLayer.Mapping;
using MongoDB.Driver;
using ServicesLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");
var client = new MongoClient(mongoSettings["ConnectionString"]);
var database = client.GetDatabase(mongoSettings["DatabaseName"]);
var collection = database.GetCollection<DataLayer.Models.Wallapop.Root>(mongoSettings["CollectionName"]);

builder.Services.AddSingleton(collection);

// **Registrar AutoMapper**
// Si tienes perfiles de mapeo, debes añadirlos aquí

builder.Services.AddAutoMapper(typeof(WallapopMappingProfile));
builder.Services.AddAutoMapper(typeof(VintedMappingProfile));
builder.Services.AddAutoMapper(typeof(CochesNetMappingProfile));


// Registro de tus servicios
builder.Services.AddScoped<IWeb1Data, Web1Data>(); // Registro de la capa de datos
builder.Services.AddScoped<IWeb1Service, Web1Service>(); // Registro del servicio
builder.Services.AddScoped<IWeb2Data, Web2Data>(); // Registro de la capa de datos
builder.Services.AddScoped<IWeb2Service, Web2Service>(); // Registro del servicio
builder.Services.AddScoped<IWeb3Data, Web3Data>(); // Registro de la capa de datos
builder.Services.AddScoped<IWeb3Service, Web3Service>(); // Registro del servicio
builder.Services.AddScoped<IWeb4Data, Web4Data>(); // Registro de la capa de datos
builder.Services.AddScoped<IWeb4Service, Web4Service>(); // Registro del servicio
builder.Services.AddScoped<IWebMixerService, WebMixerService>(); // Registro de la capa de datos
// Registro del servicio
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

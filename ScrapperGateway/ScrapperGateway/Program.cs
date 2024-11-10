using AutoMapper;
using DataLayer;
using DataLayer.Mappers;
using DataLayer.Models;
using DataLayer.Mapping;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using ServicesLayer;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL database connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

// Configure MongoDB
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");
var client = new MongoClient(mongoSettings["ConnectionString"]);
var database = client.GetDatabase(mongoSettings["DatabaseName"]);
var collection = database.GetCollection<DataLayer.Models.Wallapop.Root>(mongoSettings["CollectionName"]);

builder.Services.AddSingleton(collection);

// ** Register AutoMapper **
// If you have mapping profiles, you need to add them here
builder.Services.AddAutoMapper(typeof(AdMappingProfile), typeof(WallapopMappingProfile), typeof(VintedMappingProfile), typeof(CochesNetMappingProfile));


// Register Data and Service layers
builder.Services.AddScoped<IWeb1Data, Web1Data>(); // Data layer for Web1
builder.Services.AddScoped<IWeb1Service, Web1Service>(); // Service for Web1
builder.Services.AddScoped<IWeb2Data, Web2Data>(); // Data layer for Web2
builder.Services.AddScoped<IWeb2Service, Web2Service>(); // Service for Web2
builder.Services.AddScoped<IWeb3Data, Web3Data>(); // Data layer for Web3
builder.Services.AddScoped<IWeb3Service, Web3Service>(); // Service for Web3
builder.Services.AddScoped<IWeb4Data, Web4Data>(); // Data layer for Web4
builder.Services.AddScoped<IWeb4Service, Web4Service>(); // Service for Web4
builder.Services.AddScoped<IWebMixerService, WebMixerService>(); // Mixer service

// Register HTTP client
builder.Services.AddHttpClient();

// Configure CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
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

app.UseCors(); // Enable CORS

app.UseHttpsRedirection(); // Ensure HTTPS requests

app.UseAuthorization(); // Enable authorization middleware

app.MapControllers(); // Map controller routes

app.Run(); // Start the application

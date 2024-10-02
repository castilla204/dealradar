using DataLayer;
using ServicesLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// **AQUÍ** es donde debes registrar tus servicios
builder.Services.AddScoped<IWeb1Data, Web1Data>(); // Registro de la capa de datos
builder.Services.AddScoped<IWeb1Service, Web1Service>(); // Registro del servicio
builder.Services.AddScoped<IWeb2Data, Web2Data>(); // Registro de la capa de datos
builder.Services.AddScoped<IWeb2Service, Web2Service>(); // Registro del servicio
builder.Services.AddScoped<IWeb3Data, Web3Data>(); // Registro de la capa de datos
builder.Services.AddScoped<IWeb3Service, Web3Service>(); // Registro del servicio

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SGMA.Application.Mapper;
using SGMA.Infrastructure.Data;
using SGMA.WebAPI.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// La cadena de conexión vive en User Secrets (ver README, paso 2)
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Mapster: Pattern Mapper
// 1. Trigger custom configurations
MapsterConfig.RegisterMaps();
// 2. Grab global settings
var config = TypeAdapterConfig.GlobalSettings;
// 3. Register Mapster into the Service Collection
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddMapster();

// Manejo global de excepciones: toda respuesta de error sale como ProblemDetails en español
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Título en español para los errores de validación de los DTO (400 automático de [ApiController])
        if (context.ProblemDetails is HttpValidationProblemDetails)
            context.ProblemDetails.Title = "Uno o más datos enviados no son válidos";
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
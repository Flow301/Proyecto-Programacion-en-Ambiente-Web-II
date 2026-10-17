using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SGMA.Application.Implementation;
using SGMA.Application.Interfaces;
using SGMA.Application.Mapper;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Implementation;
using SGMA.Infrastructure.Interfaces;
using SGMA.WebAPI.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Seeders en tiempo de ejecución, ver docs/modelo-datos.md
// El DevelopmentSeeder solo corre en Development. Cuando exista el MasterSeeder (admin),
// va primero en los dos bloques, con la contraseña de Seed:AdminPassword.
var isDevelopment = builder.Environment.IsDevelopment();

// La cadena de conexión vive en User Secrets (ver README, paso 2)
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    // Las herramientas de EF (dotnet ef database update) usan la versión síncrona
    .UseSeeding((context, _) =>
    {
        if (isDevelopment) DevelopmentSeeder.Seed(context);
    })
    // La aplicación (Database.MigrateAsync) usa la versión asíncrona
    .UseAsyncSeeding(async (context, _, ct) =>
    {
        if (isDevelopment) await DevelopmentSeeder.SeedAsync(context, ct);
    }));

// Mapster: Pattern Mapper
// 1. Trigger custom configurations
MapsterConfig.RegisterMaps();
// 2. Grab global settings
var config = TypeAdapterConfig.GlobalSettings;
// 3. Register Mapster into the Service Collection
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddMapster();

// Repositorios y servicios
builder.Services.AddScoped<ICategoriaActivoRepository, CategoriaActivoRepository>();
builder.Services.AddScoped<IActivoRepository, ActivoRepository>();
builder.Services.AddScoped<ICategoriaActivoService, CategoriaActivoService>();
builder.Services.AddScoped<IActivoService, ActivoService>();
builder.Services.AddScoped<IEspecialidadRepository, EspecialidadRepository>();
builder.Services.AddScoped<IEspecialidadService, EspecialidadService>();
builder.Services.AddScoped<ITecnicoRepository, TecnicoRepository>();
builder.Services.AddScoped<ITecnicoService, TecnicoService>();

// Manejo global de excepciones: los errores responden con Result<T> en español; solo la validación de DTO (400) usa ValidationProblemDetails
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
    // Menú y búsqueda de Scalar muestran la ruta en lugar del EndpointSummary
    app.MapScalarApiReference(options => options
        .WithOperationTitleSource(OperationTitleSource.Path));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
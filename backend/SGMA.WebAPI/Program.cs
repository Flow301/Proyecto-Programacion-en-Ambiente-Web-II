using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SGMA.Application.Implementation;
using SGMA.Application.Interfaces;
using SGMA.Application.Mapper;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Implementation;
using SGMA.Infrastructure.Interfaces;
using SGMA.WebAPI.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // El 400 automático de [ApiController] usa la misma forma Result<T> que el resto de la API (como en el material)
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value!.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            var result = Result<Dictionary<string, string[]>>.Failure(400, "Solicitud inválida",
                "Uno o más datos enviados no son válidos.");
            result.Data = errors;

            return new BadRequestObjectResult(result);
        };
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Seeders en tiempo de ejecución, ver docs/modelo-datos.md
// Primero el MasterSeeder (admin, con la contraseña de Seed:AdminPassword) y después,
// solo en Development, el DevelopmentSeeder.
var isDevelopment = builder.Environment.IsDevelopment();
var adminPassword = builder.Configuration["Seed:AdminPassword"];

// La cadena de conexión y la contraseña del admin viven en User Secrets (ver README, paso 2)
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    // Las herramientas de EF (dotnet ef database update) usan la versión síncrona
    .UseSeeding((context, _) =>
    {
        MasterSeeder.Seed(context, adminPassword);
        if (isDevelopment) DevelopmentSeeder.Seed(context);
    })
    // La aplicación (Database.MigrateAsync) usa la versión asíncrona
    .UseAsyncSeeding(async (context, _, ct) =>
    {
        await MasterSeeder.SeedAsync(context, adminPassword, ct);
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
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IRepuestoRepository, RepuestoRepository>();
builder.Services.AddScoped<IRepuestoService, RepuestoService>();

// Manejo global de excepciones: los errores responden con Result<T> en español.
// UseExceptionHandler() sin ruta exige AddProblemDetails, aunque GlobalExceptionHandler siempre responde primero.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Aplica las migraciones pendientes y dispara UseAsyncSeeding (ver docs/modelo-datos.md, "Seeders")
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

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

// Sirve las imágenes subidas: /imagenes/activos/<código>.<ext> (wwwroot)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
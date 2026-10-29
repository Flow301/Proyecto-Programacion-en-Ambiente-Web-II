using System.Text;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
using SGMA.WebAPI.Security;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Serilog reemplaza al proveedor de logging por defecto (como en el material)
builder.Services.AddSerilog((services, logConfig) => logConfig
    .MinimumLevel.Information()                                             // nivel general
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)              // menos ruido del framework
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information) // "Now listening on..."
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
        builder.Environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning) // SQL de EF solo en Development
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/serilog-.txt",
        rollingInterval: RollingInterval.Day,   // un archivo por día: serilog-20261024.txt
        retainedFileCountLimit: 14,             // conserva 14 archivos
        fileSizeLimitBytes: 10_485_760,         // 10 MB por archivo
        rollOnFileSizeLimit: true,              // al llegar a 10 MB abre serilog-20261024_001.txt
        shared: true,                           // varios procesos pueden escribir el mismo archivo
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}"));

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // El 400 automático de [ApiController] usa la misma forma Result<T> que el resto de la API (como en el material)
        options.InvalidModelStateResponseFactory = context =>
        {
            // Errores por campo; los de un cuerpo vacío o un JSON mal formado se traducen al español
            var errors = ErroresDeModelo.EnEspanol(context);

            var result = Result<Dictionary<string, string[]>>.Failure(400, "Solicitud inválida",
                "Uno o más datos enviados no son válidos.");
            result.Data = errors;

            return new BadRequestObjectResult(result);
        };
    });
// Documento OpenAPI con título, versión y descripción (como en el material)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "SGMA API",
            Version = "v1",
            Description = "API del Sistema de Gestión de Mantenimiento de Activos (ISW-621, UTN). " +
                "Todas las respuestas usan Result<T>: isSuccess, status, title, detail y data. " +
                "Salvo POST api/Auth/login, todo endpoint pide el token en el encabezado Authorization: Bearer {token}."
        };

        // Esquema Bearer: Scalar muestra dónde pegar el token del login
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Token de POST api/Auth/login."
        };
        document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }];
        return Task.CompletedTask;
    });
});

// JWT: la clave vive en User Secrets (ver README, paso 2); emisor, audiencia y duración en appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Falta la configuración Jwt:Key o tiene menos de 32 caracteres. Ver README, paso 2.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // los claims conservan su nombre corto: sub, name, role
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = ClaimsPrincipalExtensions.RoleClaim,
            ClockSkew = TimeSpan.FromMinutes(1) // por defecto son 5: el token vencería 5 minutos tarde
        };

        // 401 y 403 con la misma forma Result<T> que el resto de la API
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse(); // reemplaza la respuesta vacía por defecto
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsJsonAsync(Result<object>.Failure(401, "No autorizado",
                    "Debe iniciar sesión: el token no se envió, no es válido o ya expiró."));
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsJsonAsync(Result<object>.Failure(403, "Acceso denegado",
                    "Su rol no tiene permiso para realizar esta acción."));
            }
        };
    });

// Todo endpoint exige token salvo los marcados con [AllowAnonymous]: ninguno queda abierto por olvido.
// Los roles de cada acción se indican con [Authorize(Roles = ...)] en los controladores.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

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
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IRepuestoRepository, RepuestoRepository>();
builder.Services.AddScoped<IRepuestoService, RepuestoService>();
builder.Services.AddScoped<IOrdenMantenimientoRepository, OrdenMantenimientoRepository>();
builder.Services.AddScoped<IOrdenMantenimientoService, OrdenMantenimientoService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseSerilogRequestLogging(); // una línea de log por petición: método, ruta, código y duración
app.UseExceptionHandler();      // todo lo que viene después queda protegido por el manejador global

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // La documentación queda pública; los endpoints que describe piden token
    app.MapOpenApi().AllowAnonymous();
    // Menú y búsqueda de Scalar muestran la ruta en lugar del <summary> de cada acción
    app.MapScalarApiReference(options => options
        .WithTitle("SGMA API")
        .WithOperationTitleSource(OperationTitleSource.Path)
        .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)) // ejemplos de consumo para React
        .AllowAnonymous();
}

app.UseHttpsRedirection();

// Sirve las imágenes subidas: /imagenes/activos/<código>.<ext> (wwwroot)
// Antes de la autenticación: las imágenes no piden token (las carga un <img> del frontend)
app.UseStaticFiles();

app.UseAuthentication(); // lee y valida el token del encabezado Authorization
app.UseAuthorization();  // aplica [Authorize] y la política por defecto

app.MapControllers();

app.Run();
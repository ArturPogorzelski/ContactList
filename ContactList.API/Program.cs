using ContactList.API.Configuration;
using ContactList.API.Extensions;
using ContactList.API.Midleware;
using ContactList.Authentication.Models;
using ContactList.Authentication.Services;
using ContactList.Core.Interfaces;
using ContactList.Infrastructure.Data.Contexts;
using ContactList.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Text;
using ContactList.API.Helpers;



var builder = WebApplication.CreateBuilder(args);
// Tworzy nowy obiekt WebApplicationBuilder, kt�ry s�u�y do skonfigurowania i uruchomienia aplikacji webowej.

builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new RenderedCompactJsonFormatter(), "Logs/log-.txt", rollingInterval: RollingInterval.Day));


// Konfiguracja uwierzytelniania JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("GatewayAuthenticationScheme", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    c.OperationFilter<AuthorizationHeaderOperationFilter>();
});

// Konfiguruje Serilog jako system logowania dla hosta. Ustawia minimalny poziom logowania dla przestrzeni nazw Microsoft, dodaje kontekst logowania i konfiguruje zapisywanie log�w do konsoli oraz do pliku z dziennym rolowaniem.

builder.Host.UseSerilog();
// Rejestruje Serilog jako domy�lny system logowania.

builder.Services.AddDbContext<ContactListDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ContactListDb")));
// Dodaje i konfiguruje kontekst bazy danych Entity Framework Core dla po��czenia z baz� danych SQL Server.

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
// Pobiera konfiguracj� JWT z appsettings.json i rejestruje j� jako konfigurowaln� opcj�.

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            ClockSkew = TimeSpan.Zero
        };
    });
// Konfiguruje uwierzytelnianie przy u�yciu JWT, w tym weryfikacj� klucza, wydawcy i odbiorcy tokena.

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    // Tworzy dokumentacj� Swagger dla API, zdefiniowan� dla wersji v1.
    // Dalej konfiguruje uwierzytelnianie JWT w dokumentacji Swagger.
});

builder.Services.AddPersistence(builder.Configuration);
// Metoda dodatkowa zdefiniowana przez u�ytkownika, prawdopodobnie dodaje klasy dotycz�ce persystencji danych.

builder.Services.AddServices();
// Rejestruje dodatkowe serwisy zdefiniowane przez u�ytkownika.

builder.Services.AddValidation();
// Rejestruje serwisy odpowiedzialne za walidacj� danych wej�ciowych.

builder.Services.Configure<RetryPolicyConfig>(builder.Configuration.GetSection("RetryPolicy"));
// Konfiguruje polityk� ponawiania operacji w przypadku wyst�pienia b��d�w.

builder.Services.AddControllers();
// Rejestruje serwisy odpowiedzialne za obs�ug� kontroler�w MVC.

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Konfiguruje AutoMapper, narz�dzie do mapowania danych mi�dzy obiektami, u�ywaj�c wszystkich zgromadzonych w aplikacji.

var app = builder.Build();
// Buduje i konfiguruje aplikacj�.

app.UseMiddleware<LoggingMiddleware>();
// Rejestruje middleware do logowania.

app.UseMiddleware<JwtDebugMiddleware>();
// Rejestruje middleware do debugowania JWT.

app.UseHttpsRedirection();
// Wymusza przekierowanie HTTP na HTTPS.

app.UseAuthentication();
// W��cza mechanizmy uwierzytelniania.

app.UseAuthorization();
// W��cza mechanizmy autoryzacji.

app.UseMiddleware<ErrorHandlingMiddleware>();
// Rejestruje middleware do obs�ugi b��d�w.

app.MapControllers();
// Mapuje kontrolery do endpoint�w HTTP.

app.Run();
// Uruchamia aplikacj�.
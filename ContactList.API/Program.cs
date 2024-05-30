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
using ContactList.Application.Commands;
using Microsoft.Extensions.DependencyInjection;
using ContactList.Application.Mappings;



var builder = WebApplication.CreateBuilder(args);
// Konfiguracja Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new RenderedCompactJsonFormatter(), "Logs/log-.txt", rollingInterval: RollingInterval.Day));

builder.Host.UseSerilog(); // Informujemy ASP.NET Core, aby u¿ywa³ Serilog jako systemu logowania

builder.Services.AddDbContext<ContactListDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ContactListDb")));

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));




// Dodaj us³ugê autoryzacji i uwierzytelniania JWT
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

// Rejestracja AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperCQRSProfile));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddPersistence(builder.Configuration); // Wywo³aj AddPersistence z konfiguracj¹
builder.Services.AddServices();
builder.Services.AddValidation();
builder.Services.AddCQRSHandlers();
builder.Services.Configure<RetryPolicyConfig>(builder.Configuration.GetSection("RetryPolicy"));
builder.Services.AddControllers();




builder.Services.AddControllers();




//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}


app.UseMiddleware<LoggingMiddleware>(); // Dodaj middleware logowania nag³ówków
app.UseMiddleware<JwtDebugMiddleware>(); // Dodaj middleware Debugowania Jwt

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();
//app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();

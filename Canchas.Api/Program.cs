using Canchas.Api.Data;
using Canchas.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONTROLADORES
// =====================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// =====================================================
// SWAGGER + JWT
// =====================================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Canchas API",
            Version = "v1",
            Description = "API REST para el Sistema de Reservación de Canchas"
        }
    );

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingrese únicamente el token JWT."
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        }
    );
});

// =====================================================
// CORS
// =====================================================

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ??
    new[]
    {
        "http://localhost:5173"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

// =====================================================
// SQL SERVER + ENTITY FRAMEWORK CORE
// =====================================================

var connectionString =
    builder.Configuration
        .GetConnectionString("CanchasDb")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión CanchasDb."
    );

builder.Services.AddDbContext<CanchasDbContext>(
    options =>
    {
        options.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null
                );
            }
        );
    }
);

// =====================================================
// SERVICIOS
// =====================================================

builder.Services.AddScoped<TokenService>();

// =====================================================
// JWT
// =====================================================

var jwtKey =
    builder.Configuration["JwtSettings:Key"]
    ?? throw new InvalidOperationException(
        "No se encontró JwtSettings:Key."
    );

var jwtIssuer =
    builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException(
        "No se encontró JwtSettings:Issuer."
    );

var jwtAudience =
    builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException(
        "No se encontró JwtSettings:Audience."
    );

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

// =====================================================
// RATE LIMITING
// =====================================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(
        policyName: "login",
        fixedWindowOptions =>
        {
            fixedWindowOptions.PermitLimit = 10;

            fixedWindowOptions.Window =
                TimeSpan.FromMinutes(1);

            fixedWindowOptions.QueueLimit = 0;

            fixedWindowOptions.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;

            fixedWindowOptions.AutoReplenishment = true;
        }
    );
});

// =====================================================
// CONSTRUIR APLICACIÓN
// =====================================================

var app = builder.Build();

// =====================================================
// INICIALIZAR DATOS
// =====================================================

await InicializadorDatos.InicializarAsync(
    app.Services,
    app.Configuration
);

// =====================================================
// SWAGGER
// =====================================================

var swaggerEnabled =
    app.Environment.IsDevelopment()
    ||
    app.Configuration
        .GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =====================================================
// PIPELINE HTTP
// =====================================================

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Auth;
using Infrastructure.Cache;
using Infrastructure.Messaging;
using Infrastructure.Persistence.Postgres;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using StackExchange.Redis; // Add this using directive


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LancamentoDtoValidator>();

// Configuração do Redis
builder.Services.AddSingleton<RedisConfig>(sp =>
{
    var config = new RedisConfig
    {
        ConnectionString = builder.Configuration["REDIS_CONNECTION_STRING"]
            ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
            ?? "redis:6379",
        InstanceName = builder.Configuration["REDIS_INSTANCE_NAME"]
            ?? Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME")
            ?? "Lancamentos_",
        CacheTimeoutMinutes = int.TryParse(
            builder.Configuration["REDIS_CACHE_TIMEOUT_MINUTES"]
            ?? Environment.GetEnvironmentVariable("REDIS_CACHE_TIMEOUT_MINUTES"),
            out var timeout)
            ? timeout
            : 30
    };
    return config;
});

// Configuração do DistributedCache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["REDIS_CONNECTION_STRING"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
        ?? "redis:6379";
    options.InstanceName = builder.Configuration["REDIS_INSTANCE_NAME"]
        ?? Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME")
        ?? "Lancamentos_";
});

// Configuração do ConnectionMultiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(
        builder.Configuration["REDIS_CONNECTION_STRING"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
        ?? "redis:6379"));
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lancamentos API", Version = "v1" });
    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authorization header"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddSingleton<IRepository<Core.Domain.Entities.Lancamento>>(
    new LancamentoRepository(builder.Configuration.GetConnectionString("Postgres")));

// RabbitMQ
builder.Services.AddRabbitMQ(
    builder.Configuration["RabbitMQ:Host"],
    builder.Configuration["RabbitMQ:Username"],
    builder.Configuration["RabbitMQ:Password"]);


// Services
builder.Services.AddScoped<Core.Application.Interfaces.ILancamentoService, LancamentoService>();

// Authentication
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

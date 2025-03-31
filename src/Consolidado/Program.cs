using Consolidado.Consumers;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Interfaces;
using Infrastructure.Auth;
using Infrastructure.Messaging;
using Infrastructure.Persistence.Mongo;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Consolidado API", Version = "v1" });
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
builder.Services.AddSingleton<IRepository<Core.Domain.Entities.ConsolidadoDiario>>(sp =>
    new ConsolidadoRepository(
        builder.Configuration.GetConnectionString("Mongo"),
        builder.Configuration["Mongo:DatabaseName"]));
// RabbitMQ
//builder.Services.AddRabbitMQ(
//    builder.Configuration["RabbitMQ:Host"],
//    builder.Configuration["RabbitMQ:Username"],
//    builder.Configuration["RabbitMQ:Password"]);
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<LancamentoRegistradoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ReceiveEndpoint("lancamento-registrado", e =>
        {
            e.ConfigureConsumer<LancamentoRegistradoConsumer>(context);
        });
    });
});
// Services
builder.Services.AddScoped<IConsolidadoService, ConsolidadoService>();

// Authentication
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging;

public static class RabbitMQExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string host, string username, string password)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

            });

        });

        return services;
    }
}
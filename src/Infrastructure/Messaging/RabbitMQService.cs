using MassTransit;
using Polly;
using System.Net.Sockets;

namespace Infrastructure.Messaging;

public class RabbitMQService
{
    private readonly IBus _bus;
    private readonly IAsyncPolicy _retryPolicy;

    public RabbitMQService(IBus bus)
    {
        _bus = bus;
        _retryPolicy = Policy
            .Handle<SocketException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public async Task Publish<T>(T message) where T : class
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _bus.Publish(message);
        });
    }
}
using FiapX.Domain.Interfaces;
using MassTransit;

namespace FiapX.Infrastructure.Messaging;

/// <summary>
/// Implementação do IMessagePublisher usando MassTransit/RabbitMQ.
/// Application não conhece MassTransit — só conhece a interface IMessagePublisher do Domain.
/// </summary>
public class MassTransitMessagePublisher : IMessagePublisher
{
    private readonly IBus _bus;

    public MassTransitMessagePublisher(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await _bus.Publish(message, cancellationToken);
    }
}
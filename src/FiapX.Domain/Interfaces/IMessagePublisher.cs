namespace FiapX.Domain.Interfaces;

/// <summary>
/// Interface para publicação de mensagens/eventos.
/// Definida no Domain para que Application possa publicar eventos sem referenciar MassTransit diretamente.
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
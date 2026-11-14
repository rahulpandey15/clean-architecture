namespace eCommerce.Application.Common.Messaging;

public interface IOutboxWriter
{
    Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}

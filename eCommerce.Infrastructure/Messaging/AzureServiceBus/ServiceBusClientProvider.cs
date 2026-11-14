using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace eCommerce.Infrastructure.Messaging.AzureServiceBus;

public sealed class ServiceBusClientProvider(IOptions<ServiceBusOptions> options) : IAsyncDisposable
{
    private readonly ServiceBusOptions _options = options.Value;
    private ServiceBusClient? _client;

    public ServiceBusClient GetClient()
    {
        if (_client is not null)
        {
            return _client;
        }

        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            _client = new ServiceBusClient(
                _options.ConnectionString,
                new ServiceBusClientOptions
                {
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        MaxRetries = _options.RetryMaxRetries,
                        Delay = _options.RetryDelay,
                        MaxDelay = TimeSpan.FromMinutes(1),
                        Mode = ServiceBusRetryMode.Exponential
                    }
                });
        }
        else if (!string.IsNullOrWhiteSpace(_options.FullyQualifiedNamespace))
        {
            _client = new ServiceBusClient(
                _options.FullyQualifiedNamespace,
                new DefaultAzureCredential(),
                new ServiceBusClientOptions
                {
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        MaxRetries = _options.RetryMaxRetries,
                        Delay = _options.RetryDelay,
                        MaxDelay = TimeSpan.FromMinutes(1),
                        Mode = ServiceBusRetryMode.Exponential
                    }
                });
        }
        else
        {
            throw new InvalidOperationException(
                "Azure Service Bus is enabled but neither ConnectionString nor FullyQualifiedNamespace is configured.");
        }

        return _client;
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.DisposeAsync();
        }
    }
}

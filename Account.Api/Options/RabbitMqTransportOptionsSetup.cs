using Microsoft.Extensions.Options;
using MassTransit;

namespace Account.Api.Options;

public class RabbitMqTransportOptionsSetup : IConfigureOptions<RabbitMqTransportOptions>
{
    private readonly EnvironmentVariableOptions _environmentVariables;

    public RabbitMqTransportOptionsSetup(IOptions<EnvironmentVariableOptions> options)
    {
        _environmentVariables = options.Value;
    }

    public void Configure(RabbitMqTransportOptions options)
    {
        options.Host = _environmentVariables.TransportHost;
        options.User = _environmentVariables.TransportUsername;
        options.Pass = _environmentVariables.TransportPassword;
    }
}

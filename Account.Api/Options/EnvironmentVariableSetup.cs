using Microsoft.Extensions.Options;

namespace Account.Api.Options;

public class EnvironmentVariableSetup : IConfigureOptions<EnvironmentVariableOptions>
{
    public const string TRANSPORT_HOST = "RABBITMQ_HOST";
    public const string TRANSPORT_USERNAME = "RABBITMQ_USERNAME";
    public const string TRANSPORT_PASSWORD = "RABBITMQ_PASSWORD";

    private static Dictionary<string, string> EnvAndProperties => new Dictionary<string, string>()
    {
        { TRANSPORT_HOST, nameof(EnvironmentVariableOptions.TransportHost) },
        { TRANSPORT_USERNAME, nameof(EnvironmentVariableOptions.TransportUsername) },
        { TRANSPORT_PASSWORD, nameof(EnvironmentVariableOptions.TransportPassword) }
    };

    public void Configure(EnvironmentVariableOptions options)
    {
        foreach (var variable in EnvAndProperties)
        {
            string? value = Environment.GetEnvironmentVariable(variable.Key);
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception($"Missing {variable.Key} environment variable");
            }

            var propertyInfo = options.GetType().GetProperty(variable.Value)!;
            Type t = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            object? safeValue = value == null ? null : Convert.ChangeType(value, t);
            propertyInfo.SetValue(options, safeValue, null);
        }
    }
}

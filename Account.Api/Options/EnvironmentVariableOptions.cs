namespace Account.Api.Options;

public class EnvironmentVariableOptions
{
    public string TransportHost { get; set; } = null!;
    public string TransportUsername { get; set; } = null!;
    public string TransportPassword { get; set; } = null!;
}

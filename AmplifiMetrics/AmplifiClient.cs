namespace AmplifiMetrics;

public class AmplifiClient
{
    private readonly HttpClient _client;
    private readonly string _defaultGateway;

    public AmplifiClient(HttpClient client, string defaultGateway)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultGateway);
        
        _client = client;
        _defaultGateway = defaultGateway;
    }
}

public class AmplifiAuthHandler : DelegatingHandler
{
    private readonly string _password;
    public AmplifiAuthHandler(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        _password = password;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return base.SendAsync(request, cancellationToken);
    }
}
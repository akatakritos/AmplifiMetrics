namespace AmplifiMetrics;

public class AmplifiClient
{
    private readonly HttpClient _client;
    public AmplifiClient(HttpClient client)
    {
        _client = client;
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
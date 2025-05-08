using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;

namespace AmplifiMetrics;

public partial class AmplifiClient
{
    private readonly HttpClient _client;
    private readonly string _routerPassword;
    private readonly CookieContainer _cookieContainer;
    private readonly ILogger<AmplifiClient> _logger;
    private static string? _cachedInfoToken;

    public AmplifiClient(HttpClient client, string routerPassword, CookieContainer cookieContainer, ILogger<AmplifiClient> logger)
    {
        _client = client;
        _routerPassword = routerPassword;
        _cookieContainer = cookieContainer;
        _logger = logger;
    }

    public async Task<JsonNode> GetMetrics()
    {
        try
        {
            await EnsureLogin();

            if (_cachedInfoToken is null)
            {
                var html = await _client.GetStringAsync("/info.php");
                if (TokenRegex.Match(html) is { Success: true } match)
                {
                    _cachedInfoToken = match.Groups[1].Value;
                }
                else
                {
                    throw new Exception("Could not find token");
                }
            }

            Debug.Assert(_cachedInfoToken is not null);
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/info-async.php")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "token", _cachedInfoToken! },
                    { "do", "full" }
                })
            });

            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonNode.ParseAsync(stream);
            Debug.Assert(json is not null);
            return json;
        }
        catch
        {
            _logger.LogWarning("Crashed while fetching metrics, resetting auth state for next scrape");
            // reset all our auth data so that we renew it next time prometheus scrapes us
            ResetAuthState();
            
            throw;
        }
    }
    
    [GeneratedRegex("var token='([0-9a-f]+)'")]
    private static partial Regex TokenRegex { get; }

    private void ResetAuthState()
    {
        _cachedInfoToken = null;
        foreach (Cookie cookie in _cookieContainer.GetCookies(_client.BaseAddress!))
            cookie.Expired = true;
        _logger.LogInformation("Reset auth state");
    }

    private async Task EnsureLogin()
    {
        if (_cookieContainer.Count > 0) return;
        
        var html = await _client.GetStringAsync("/login.php");
        var parser = new HtmlParser();
        var document = parser.ParseDocument(html);
        var token = document.QuerySelector("input[name=token]")?.GetAttribute("value");
        Debug.Assert(token is not null);
        
        // should set cookie
        var loginResponse = await _client.PostAsync("/login.php", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"token", token},
            {"password", _routerPassword}
        }));
        
        loginResponse.EnsureSuccessStatusCode();
        Debug.Assert(_cookieContainer.Count == 1);

    }
}
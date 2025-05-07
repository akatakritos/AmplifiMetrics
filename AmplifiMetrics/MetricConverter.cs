using System.Text.Json.Nodes;

namespace AmplifiMetrics;

public enum MetricType
{
    Guage,
    Counter
}

public record Metric(MetricType Type, string Name, string Help, double Value, string Device);

public static class MetricNames
{
    public const string HappinessScore = "happiness_score";
    public const string ReceivedBytes = "received_bytes";
    public const string TransmittedBytes = "transmitted_bytes";
    public const string Uptime = "uptime_seconds";
}

public static class MetricConverter
{
    public static IEnumerable<Metric> Parse(string metricsJson)
    {
        var root = JsonNode.Parse(metricsJson);
        if (root is not JsonArray rootArray) yield break;

        var router = rootArray[0];
        var routerMac = router[0].GetPropertyName();
        var uptime = router[0]["uptime"].GetValue<double>();
        var friendlyName = router[0]["friendly_name"].GetValue<string>();
        yield return new Metric(MetricType.Counter, MetricNames.Uptime, "Seconds since router was last rebooted",
            uptime, friendlyName);

        if (root[1] is not JsonObject devices) yield break;
        if (devices[routerMac]?["2.4 GHz"]?["User network"] is JsonObject devices24)
        {
            foreach (var metric in devices24.SelectMany(kvp => ParseDevice(kvp.Key, (JsonObject)kvp.Value)))
            {
                yield return metric;
            }
        }

        if (devices[routerMac]?["5 GHz"]?["User network"] is JsonObject devices5)
        {
            foreach (var metric in devices5.SelectMany(kvp => ParseDevice(kvp.Key, (JsonObject)kvp.Value)))
            {
                yield return metric;
            }
        }
    }

    private static IEnumerable<Metric> ParseDevice(string mac, JsonObject device)
    {
        var deviceName = device["Description"]?.GetValue<string>()
                         ?? device["HostName"]?.GetValue<string>()
                         ?? mac;


        if (device["HappinessScore"] is JsonValue happinessScore)
        {
            yield return new Metric(MetricType.Guage, MetricNames.HappinessScore, "Happiness score",
                happinessScore.GetValue<double>(), deviceName);
        }

        if (device["RxBytes"] is JsonValue rxBytes)
        {
            yield return new Metric(MetricType.Counter, MetricNames.ReceivedBytes, "Received bytes",
                rxBytes.GetValue<double>(), deviceName);
        }

        if (device["TxBytes"] is JsonValue txBytes)
        {
            yield return new Metric(MetricType.Counter, MetricNames.TransmittedBytes, "Transmitted bytes",
                txBytes.GetValue<double>(), deviceName);
        }
    }
}
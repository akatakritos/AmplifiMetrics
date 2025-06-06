using System.Text.Json.Nodes;

namespace AmplifiMetrics;

public enum MetricType
{
    Gauge,
    Counter
}

public record Metric(MetricType Type, string Name, string Help, double Value, string Device);

public static class MetricNames
{
    public const string HappinessScore = "amplifi_happiness_score";
    public const string ReceivedBytes = "amplifi_received_bytes_total";
    public const string TransmittedBytes = "amplifi_transmitted_bytes_total";
    public const string Uptime = "amplifi_uptime_seconds_total";
    public const string EthernetBitrateTx = "amplifi_ethernet_bitrate_tx";
    public const string EthernetBitrateRx = "amplifi_ethernet_bitrate_rx";
}

public static class MetricConverter
{
    public static IEnumerable<Metric> Parse(string json) => Parse(JsonNode.Parse(json));
    public static IEnumerable<Metric> Parse(JsonNode root)
    {
        ArgumentNullException.ThrowIfNull(root);
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

        if (root[4] is not JsonObject ethernet) yield break;
        var ports = (JsonObject)ethernet[routerMac];
        foreach (var port in ports)
        {
            if (port.Value["link"].GetValue<bool>())
            {
                if (port.Value["rx_bitrate"] is JsonValue rxBitrate)
                    yield return new Metric(MetricType.Gauge, MetricNames.EthernetBitrateRx,
                        "Bitrate of the port in bits per second",
                        rxBitrate.GetValue<double>(), port.Key);

                if (port.Value["tx_bitrate"] is JsonValue txBitrate)
                    yield return new Metric(MetricType.Gauge, MetricNames.EthernetBitrateTx,
                        "Bitrate of the port in bits per second",
                        txBitrate.GetValue<double>(), port.Key);
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
            yield return new Metric(MetricType.Gauge, MetricNames.HappinessScore,
                "Happiness score is a measure of device connection health",
                happinessScore.GetValue<double>(), deviceName);
        }

        if (device["RxBytes"] is JsonValue rxBytes)
        {
            yield return new Metric(MetricType.Counter, MetricNames.ReceivedBytes,
                "Total count of bytes received by the device",
                rxBytes.GetValue<double>(), deviceName);
        }

        if (device["TxBytes"] is JsonValue txBytes)
        {
            yield return new Metric(MetricType.Counter, MetricNames.TransmittedBytes,
                "Total count of bytes transmitted by the device",
                txBytes.GetValue<double>(), deviceName);
        }
    }
}
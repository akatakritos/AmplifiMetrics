using Shouldly;

namespace AmplifiMetrics.Tests;

public class MetricConverterTests
{
    [Fact]
    public void ParsePuraMetrics()
    {
        var metrics = MetricConverter.Parse(File.ReadAllText(Path.Join("Fixtures", "full.json")))
            .ToList();


         metrics.Single(m => m.Name == MetricNames.HappinessScore &&
                            m.Device == "Pura-1ACC")
            .Value
            .ShouldBe(75);
         
         metrics.Single(m => m.Name == MetricNames.ReceivedBytes &&
                             m.Device == "Pura-1ACC")
             .Value
             .ShouldBe(784686);
         
         metrics.Single(m => m.Name == MetricNames.TransmittedBytes &&
                             m.Device == "Pura-1ACC")
             .Value
             .ShouldBe(15198414);
         
    }

    [Fact]
    public void RouterUptime()
    {
        var metrics = MetricConverter.Parse(File.ReadAllText(Path.Join("Fixtures", "full.json")))
            .ToList();


        var uptime = metrics.Single(m => m.Name == MetricNames.Uptime);
        uptime.Value.ShouldBe(350132);
    }
    
    [Fact]
    public void ParseEthernetMetrics()
    {
        var metrics = MetricConverter.Parse(File.ReadAllText(Path.Join("Fixtures", "full.json")))
            .ToList();

        metrics.Single(m => m.Device == "eth-0" && m.Name == MetricNames.EthernetBitrateTx)
            .Value.ShouldBe(52);
        metrics.Single(m => m.Device == "eth-0" && m.Name == MetricNames.EthernetBitrateRx)
            .Value.ShouldBe(30);
    }
}
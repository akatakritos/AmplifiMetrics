using System.Text;

namespace AmplifiMetrics;

public static class PrometheusRenderer
{
    public static string Render(IEnumerable<Metric> metrics)
    {
        var builder = new StringBuilder();

        foreach (var metric in metrics)
        {
            builder.AppendFormat("# HELP {0} {1}\n", metric.Name, metric.Help);
            builder.AppendFormat("# TYPE {0} {1}\n", metric.Name, metric.Type.ToString().ToLower());
            builder.AppendFormat("{0}{{device=\"{1}\"}} {2}\n\n", metric.Name, metric.Device, metric.Value);
        }
        
        return builder.ToString();
    }
    
}
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace SmartLogistics.Shared.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSmartLogisticsSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, config) =>
        {
            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {TraceId} {UserId} {Message:lj}{NewLine}{Exception}")
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            var seqUrl = context.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                config.WriteTo.Seq(seqUrl);
            }

            var elasticUrl = context.Configuration["Elasticsearch:Url"];
            if (!string.IsNullOrWhiteSpace(elasticUrl))
            {
                config.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticUrl))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "smartlogistics-logs-{0:yyyy.MM.dd}"
                });
            }
        });

        return builder;
    }
}

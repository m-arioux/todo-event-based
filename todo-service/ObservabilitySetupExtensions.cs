using Confluent.Kafka.Extensions.OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace todo_service;

public static class ObservabilitySetupExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {

        // create the resource that references the service name passed in
        var resource = ResourceBuilder.CreateDefault().AddService(serviceName: "todo-service", serviceVersion: "1.0");

        // add the OpenTelemetry services
        var otelBuilder = services.AddOpenTelemetry();

        otelBuilder
            // add the metrics providers
            .WithMetrics(metrics =>
            {
                metrics
              .SetResourceBuilder(resource)
              .AddRuntimeInstrumentation()
              .AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation()
              .AddEventCountersInstrumentation(c =>
              {
                  c.AddEventSources(
                      "Microsoft.AspNetCore.Hosting",
                      "Microsoft-AspNetCore-Server-Kestrel",
                      "System.Net.Http",
                      "System.Net.Sockets");
              })
              .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
              .AddPrometheusExporter();

            })
            // add the tracing providers
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(resource)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddConfluentKafkaInstrumentation()
                        .AddZipkinExporter(zipkin =>
                        {
                            var zipkinUrl = configuration["ZIPKIN_URL"] ?? throw new Exception("Missing ZIPKIN_URL");

                            zipkin.Endpoint = new Uri($"{zipkinUrl}/api/v2/spans");
                        });
            });

        return services;
    }

    public static void MapObservability(this IEndpointRouteBuilder routes)
    {
        routes.MapPrometheusScrapingEndpoint();
    }
}

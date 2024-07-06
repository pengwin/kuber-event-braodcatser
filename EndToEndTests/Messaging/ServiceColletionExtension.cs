using EventsLib;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace EndToEndTests.Messaging;

public static class ServiceColletionExtension
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        return services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context,cfg) =>
            {
                cfg.Message<BroadCastedEvent>(e => e.SetEntityName("events"));
                cfg.Publish<BroadCastedEvent>();
            });
        });
    }
    
    public static IServiceCollection ConfigureRabbitMq(this IServiceCollection services, Action<RabbitMqTransportOptions> configure)
    {
        services.AddOptions<RabbitMqTransportOptions>()
            .Configure(configure);
        return services;
    }
}
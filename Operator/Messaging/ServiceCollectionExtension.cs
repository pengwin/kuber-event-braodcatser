using EventsLib;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Operator.Messaging.Configuration;

namespace Operator.Messaging;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddRabbitMqConfiguration(configuration)
            .AddMassTransit(x =>
            {
                x.AddConsumer<EventConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ReceiveEndpoint("events", ctx => { ctx.ConfigureConsumer<EventConsumer>(context); });
                });
            });
    }
}
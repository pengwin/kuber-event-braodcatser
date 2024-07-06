using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Operator.Messaging.Configuration;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRabbitMqConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqConfiguration>(configuration.GetSection("RabbitMq"));
        services.AddOptions<RabbitMqTransportOptions>()
            .Configure((RabbitMqTransportOptions options, IOptions<RabbitMqConfiguration> rabbitOpts) =>
            {
                options.Host = rabbitOpts.Value.Host;
                options.Port = rabbitOpts.Value.Port;
                options.User = rabbitOpts.Value.User;
                options.Pass = rabbitOpts.Value.Pass;
            });
        return services;
    }
}
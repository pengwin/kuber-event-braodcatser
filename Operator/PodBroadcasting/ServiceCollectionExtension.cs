using Microsoft.Extensions.DependencyInjection;

namespace Operator.PodBroadcasting;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPodBroadcasting(this IServiceCollection services)
    {
        return services
            .AddSingleton<PodsCollection>()
            .AddSingleton<PodNotifier>()
            .AddSingleton<PodBroadCaster>();
    }
}
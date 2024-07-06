namespace Listener.State;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddState(this IServiceCollection services)
    {
        return services.AddSingleton<ServiceState>();
    }
}
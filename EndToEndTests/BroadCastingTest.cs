using System.Diagnostics;
using System.Text.Json;
using EndToEndTests.Messaging;
using EventsLib;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EndToEndTests;

public class BroadCastingTest(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task Broadcast()
    {
        // arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var message = $"Message: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(c =>
        {
            c.ClearProviders();
            c.AddXUnit(outputHelper);
        })
        .ConfigureRabbitMq(opts =>
        {
            opts.Host = "localhost";
            opts.Port = 5672;
            opts.User = "rabbit_user";
            opts.Pass = "password";
        })
        .AddRabbitMq();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var publisher = serviceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = serviceProvider.GetRequiredService<ILogger<BroadCastingTest>>();
        
        // act
        await publisher.Publish(new BroadCastedEvent
        {
            Message = message
        }, cts.Token);
        logger.LogInformation("Message published: {Message}", message);
        
        // assert
        var listenerIp = await GetMiniKubeIp(cts.Token);
        logger.LogInformation("Listener IP: {ListenerIp}", listenerIp);
        var listenerUrl = $"http://{listenerIp}:80/state";
        var listerState = await GetListenerState(listenerUrl, logger, cts.Token);
        
        Assert.NotNull(listerState);
        Assert.Equal(message, listerState!.LastReceivedMessage);
    }

    private static async Task<string> GetMiniKubeIp(CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "minikube",
            Arguments = "ip",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.Start();

        var result = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return result.Replace("\n", string.Empty);
    }
    
    private static async Task<ListenerState?> GetListenerState(string url, ILogger logger, CancellationToken cancellationToken)
    {
        var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("host", "event-listener.example");
        var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation("Listener state: {Content}", content);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<ListenerState>(content, options);
    }
}
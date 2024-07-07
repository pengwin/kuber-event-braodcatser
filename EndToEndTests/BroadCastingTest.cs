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
    private const string EventListenerHost = "event-listener.example";
    
    [Fact]
    public async Task Broadcast()
    {
        // arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var message = $"Message: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
        
        var serviceProvider = SetupServices(outputHelper);
        var publisher = serviceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = serviceProvider.GetRequiredService<ILogger<BroadCastingTest>>();
        
        // act
        await publisher.Publish(new BroadCastedEvent
        {
            Message = message
        }, cts.Token);
        logger.LogInformation("Message published: {Message}", message);
        
        // assert
        await CheckListeners(logger, message, 10, cts.Token);
    }
    
    [Fact]
    public async Task BatchBroadcast()
    {
        const int batchSize = 1000;
        // arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(50));
        string MessageTemplate(int index) => $"Message: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}[{index}]";

        var serviceProvider = SetupServices(outputHelper);
        var publisher = serviceProvider.GetRequiredService<IPublishEndpoint>();
        
        // act
        var messages = Enumerable.Range(0, batchSize)
            .Select(MessageTemplate)
            .Select(message => new BroadCastedEvent
            {
                Message = message
            }).ToArray();
        await publisher.PublishBatch(messages, cts.Token);
        
        // assert
        const int maxAttempts = 100;
        string[]? events = [];
        for (int i = 0; i < maxAttempts; i++)
        {
            events = await GetListenerEvents(cts.Token);
            if (events?.Length == batchSize)
            {
                break;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(100), cts.Token);
        }

        Assert.NotNull(events);
        var expectedMessages = messages.Select(x => x.Message);
        var eventsSet = events.ToHashSet();
        eventsSet.IntersectWith(expectedMessages);
        Assert.Equal(messages.Length, eventsSet.Count);
    }

    private static IServiceProvider SetupServices(ITestOutputHelper outputHelper)
    {
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
        
        return serviceCollection.BuildServiceProvider();
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

    private static async Task<string> GetListenerStateUrl(CancellationToken cancellationToken)
    {
        var listenerIp = await GetMiniKubeIp(cancellationToken);
        return $"http://{listenerIp}:80/state";
    }
    
    private static async Task<string> GetListenerEventsUrl(CancellationToken cancellationToken)
    {
        var listenerIp = await GetMiniKubeIp(cancellationToken);
        return $"http://{listenerIp}:80/events";
    }
    
    private static async Task<ListenerState?> GetListenerState(string url, ILogger logger, CancellationToken cancellationToken)
    {
        var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        request.Headers.TryAddWithoutValidation("host", EventListenerHost);
        var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation("Listener state: {Content}", content);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<ListenerState>(content, options);
    }
    
    private static async Task<string[]?> GetListenerEvents(CancellationToken cancellationToken)
    {
        var client = new HttpClient();
        var url = await GetListenerEventsUrl(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        request.Headers.TryAddWithoutValidation("host", EventListenerHost);
        var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<string[]?>(content, options);
    }
    
    private static async Task CheckListeners(ILogger logger, string expectedMessage, int maxCalls, CancellationToken cancellationToken)
    {
        const int expectedNodesCount = 3;

        var listenerUrl = await GetListenerStateUrl(cancellationToken);

        var nodesState = new Dictionary<string, string>();

        for (int i = 0; i < maxCalls; i++)
        {
            var listerState = await GetListenerState(listenerUrl, logger, cancellationToken);
            Assert.NotNull(listerState);
            nodesState[listerState.PodName] = listerState.LastReceivedMessage;
            
            var nodesWithExpectedStateCount = nodesState.Count(x => x.Value == expectedMessage);
            if (nodesWithExpectedStateCount == expectedNodesCount)
            {
                return;
            }

            if (i % expectedNodesCount == 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }

        var state = string.Join(", ", nodesState.Select(x => $"{x.Key}: {x.Value}"));
        logger.LogError("Consensus not reached: {State}", state);
        throw new Exception("Consensus not reached: " + state);
    }
}
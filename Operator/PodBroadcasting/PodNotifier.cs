using System.Text;
using System.Text.Json;
using EventsLib;
using Microsoft.Extensions.Logging;

namespace Operator.PodBroadcasting;

public class PodNotifier
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public PodNotifier(HttpClient httpClient, ILogger<PodNotifier> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyPodAsync(PodInfo pod, BroadCastedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var url = pod.Url;
            var content = new StringContent(JsonSerializer.Serialize(@event), Encoding.UTF8, "application/json");
            _logger.LogInformation("Sending message to {Url}", url);
            await _httpClient.PostAsync(url, content, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to {Url}", pod.Url);
        }
    }
}
using System.Text;
using System.Text.Json;
using EventsLib;
using MassTransit;
using Microsoft.Extensions.Logging;
using Operator.PodBroadcasting;

namespace Operator.Messaging;

public class EventConsumer: IConsumer<BroadCastedEvent>
{
    private readonly PodBroadCaster _podBroadCaster;
    private readonly ILogger _logger;

    public EventConsumer(PodBroadCaster podBroadCaster, ILogger<EventConsumer> logger)
    {
        _logger = logger;
        _podBroadCaster = podBroadCaster;
    }

    public async Task Consume(ConsumeContext<BroadCastedEvent> context)
    {
        _logger.LogInformation("Received message: {Message}", context.Message.Message);
        await _podBroadCaster.BroadcastAsync(context.Message, context.CancellationToken);
    }
}
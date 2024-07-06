using EventsLib;

namespace Operator.PodBroadcasting;

public class PodBroadCaster
{
    private readonly PodNotifier _notifier;
    private readonly PodsCollection _collection;

    public PodBroadCaster(PodNotifier notifier, PodsCollection collection)
    {
        _notifier = notifier;
        _collection = collection;
    }
    
    public Task BroadcastAsync(BroadCastedEvent @event, CancellationToken cancellationToken)
    {
        var tasks = _collection.GetPods()
            .Select(pod => _notifier.NotifyPodAsync(pod, @event, cancellationToken))
            .ToArray();
        return Task.WhenAll(tasks);
    }
}
using System.Collections.Concurrent;
using k8s.Models;

namespace Operator.PodBroadcasting;

public class PodsCollection
{
    private readonly ConcurrentDictionary<string, PodInfo> _pods = new();
    
    public bool TryAddPod(V1Pod pod)
    {
        if (pod.Status.Phase != "Running")
        {
            return false;
        }
        
        if (pod.Metadata.Annotations == null)
        {
            return false;
        }
        
        if (!pod.Metadata.Annotations.TryGetValue("event-broadcast-port", out var port))
        {
            return false;
        }
        
        if (!pod.Metadata.Annotations.TryGetValue("event-broadcast-path", out var path))
        {
            return false;
        }
        
        var name = pod.Metadata.Name;
        var info = new PodInfo
        {
            Ip = pod.Status.PodIP,
            Port = port,
            Path = path
        };
        _pods.AddOrUpdate(name, info, (key, value) => info);
        return true;
    }
    
    public bool TrRemovePod(V1Pod pod)
    {
        return _pods.TryRemove(pod.Metadata.Name, out _);
    }
    
    public IEnumerable<PodInfo> GetPods()
    {
        return _pods.Values;
    }
}
using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Rbac;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Operator.PodBroadcasting;

namespace Operator.Controllers;

[EntityRbac(typeof(V1Pod), Verbs = RbacVerb.All)]
public class V1PodController : IEntityController<V1Pod>
{
    private readonly IKubernetesClient _client;
    private readonly PodsCollection _collection;
    private readonly ILogger _logger;

    public V1PodController(
        ILogger<V1PodController> logger,
        IKubernetesClient client,
        PodsCollection collection)
    {
        _logger = logger;
        _client = client;
        _collection = collection;
    }

    public Task ReconcileAsync(V1Pod entity, CancellationToken cancellationToken)
    {
        if (_collection.TryAddPod(entity))
        {
            _logger.LogInformation("Added pod {Name} {StatusPhase}.", entity.Metadata.Name, entity.Status.Phase);
        }
        
        return Task.CompletedTask;
    }

    public Task DeletedAsync(V1Pod entity, CancellationToken cancellationToken)
    {
        if (_collection.TrRemovePod(entity))
        {
            _logger.LogInformation("Pod {Name} {StatusPhase} deleted.", entity.Metadata.Name, entity.Status.Phase);
        }
        return Task.CompletedTask;
    }
}
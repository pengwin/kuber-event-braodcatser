using k8s.Models;
using KubeOps.Operator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Operator.Controllers;
using Operator.Messaging;
using Operator.PodBroadcasting;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFilter((msg, level) =>
{
    return !(msg?.Contains("KubeOps.Operator.Watcher.ResourceWatcher") ?? true);
});

builder.Services
    .AddRabbitMq(builder.Configuration)
    .AddPodBroadcasting()
    .AddKubernetesOperator()
    .AddController<V1PodController, V1Pod>();

using var host = builder.Build();
host.Run();
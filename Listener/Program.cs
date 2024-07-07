using EventsLib;
using Listener.State;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddState();
var app = builder.Build();

app.MapGet("/state", (ServiceState state) => new ListenerState
{
    LastReceivedMessage = state.LastReceivedMessage ?? "No message received",
    PodName = Environment.GetEnvironmentVariable("POD_NAME") ?? "Unknown"
    
});
app.MapPost("/event", (ILogger<Program> logger, ServiceState state, [FromBody]BroadCastedEvent @event) =>
{
    logger.LogInformation("Event: {Event.Message}", @event.Message);
    state.SetLastReceivedMessage(@event.Message);
});
app.MapGet("/events", (ServiceState state) => state.GetMessages().ToArray());

app.Run();


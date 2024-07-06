namespace Operator.PodBroadcasting;

public class PodInfo
{
    public string Ip { get; init; }
    
    public string Port { get; init; }
    
    public string Path { get; init; }
    
    public string Url => $"http://{Ip}:{Port}{Path}";
}
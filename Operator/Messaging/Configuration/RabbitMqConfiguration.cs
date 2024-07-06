namespace Operator.Messaging.Configuration;

public class RabbitMqConfiguration
{
    public string Host { get; set; }

    public ushort Port { get; set; }

    public string User { get; set; }

    public string Pass { get; set; }
}
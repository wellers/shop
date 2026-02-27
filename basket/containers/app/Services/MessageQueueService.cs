using System.Text;
using RabbitMQ.Client;

public class MessageQueueService : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private const string ExchangeName = "bookings";
    private const string QueueName = "bookings";
    private const string RoutingKey = "bookings";

    public MessageQueueService(IConfiguration configuration)
    {
        var factory = new ConnectionFactory();
        configuration.GetSection("RabbitMqConnection").Bind(factory);

        factory.AutomaticRecoveryEnabled = true;

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true);
        _channel.QueueDeclare(QueueName, true, false, false);
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
    }

    public void Publish(string message)
    {
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(ExchangeName, RoutingKey, properties, body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
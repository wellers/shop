using System.Text;
using Basket.Messages;
using Basket.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class MessageQueueService : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private const string BookingStartedExchangeName = "bookings_started";    
    private const string BookingsStartedQueueName = "bookings_started";
    private const string BookingsCompletedQueueName = "bookings_completed";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageQueueService> _logger;

    public MessageQueueService(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<MessageQueueService> logger)
    {
        _serviceProvider = serviceProvider;
		_logger = logger;

		var factory = new ConnectionFactory();
        configuration.GetSection("RabbitMqConnection").Bind(factory);

        factory.AutomaticRecoveryEnabled = true;

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

       	_channel.ExchangeDeclare(BookingStartedExchangeName, ExchangeType.Direct, true);       
        _channel.QueueBind(BookingsStartedQueueName, BookingStartedExchangeName, "");

		_channel.QueueDeclare(BookingsCompletedQueueName, true, false, false);
    }

    public void Publish(string message)
    {
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(BookingStartedExchangeName, "", properties, body);
    }

	public void StartListening()
	{
		_channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

		var consumer = new EventingBasicConsumer(_channel);

		consumer.Received += async (model, args) =>
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var context = scope.ServiceProvider.GetRequiredService<RedisService>();

				var body = args.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				var bookingCompleted = JsonConvert.DeserializeObject<BookingCompleted>(message);

				if (bookingCompleted == null)
				{
					_channel.BasicReject(args.DeliveryTag, requeue: false);
					return;
				}

				context.Database?.KeyDelete(bookingCompleted.BasketId.ToString());

				_channel.BasicAck(args.DeliveryTag, multiple: false);

				_logger.LogInformation($"[x] Deleted basket {bookingCompleted.BasketId}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing message");

				// Requeue message for retry
				_channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
			}
		};

		_channel.BasicConsume(queue: BookingsCompletedQueueName, autoAck: false, consumer: consumer);
	}

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
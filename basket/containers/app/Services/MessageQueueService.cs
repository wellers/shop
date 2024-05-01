using RabbitMQ.Client;
using System.Text;

namespace Basket.Services
{
	public class MessageQueueService
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;

		public MessageQueueService(IConfiguration configuration)
		{
			var connectionFactory = new ConnectionFactory();
			configuration.GetSection("RabbitMqConnection").Bind(connectionFactory);

			_connection = connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();

			_channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false, autoDelete: false);
		}

		public void Publish(string message)
		{
			_channel.ExchangeDeclare(exchange: "bookings", type: "direct", durable: true, autoDelete: false);
			_channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false, autoDelete: false);

			var properties = _channel.CreateBasicProperties();
			properties.Persistent = true;

			_channel.QueueBind(queue: "bookings", exchange: "bookings", routingKey: "bookings");

			var body = Encoding.UTF8.GetBytes(message);
			_channel.BasicPublish(exchange: "bookings", routingKey: "bookings", basicProperties: properties, body);

			Console.WriteLine($"Sent: {message}");
		}

		public void Dispose()
		{
			_channel.Dispose();
			_connection.Dispose();
		}
	}
}

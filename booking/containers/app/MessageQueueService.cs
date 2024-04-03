using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace Booking
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

		public void StartListening()
		{
			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += (model, args) =>
			{
				var body = args.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				Console.WriteLine(" [x] Received {0}", message);
			};

			_channel.BasicConsume(queue: "bookings", autoAck: true, consumer: consumer);
		}

		public void Dispose()
		{
			_channel.Dispose();
			_connection.Dispose();
		}
	}
}

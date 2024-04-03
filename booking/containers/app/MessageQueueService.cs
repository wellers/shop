using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Booking
{
	public class BasketPurchase
	{
		public Guid BasketId { get; set; }
		public List<int> Movies { get; set; } = [];
	}

	public class MessageQueueService
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly BookingDbContext _context;

		public MessageQueueService(BookingDbContext context, IConfiguration configuration)
		{
			_context = context;

			var connectionFactory = new ConnectionFactory();
			configuration.GetSection("RabbitMqConnection").Bind(connectionFactory);

			_connection = connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();

			_channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false, autoDelete: false);
		}

		public void StartListening()
		{
			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += async (model, args) =>
			{
				var body = args.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				BasketPurchase basketPurchase = null;
				try
				{
					basketPurchase = JsonConvert.DeserializeObject<BasketPurchase>(message);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					return;
				}

				if (basketPurchase != null)
					return;

				var movies = _context.Movies.Where(m => basketPurchase.Movies.Contains(m.MovieId)).ToList();

				await _context.Bookings.AddAsync(new Booking
				{
					BasketId = basketPurchase.BasketId,
					Movies = movies,
					BookingDate = DateTime.Now
				});

				await _context.SaveChangesAsync();

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

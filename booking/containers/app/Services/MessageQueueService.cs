using Booking.Dtos;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Booking.Messages;

namespace Booking.Services;

public class MessageQueueService : IDisposable
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IConnection _connection;
	private readonly IModel _channel;
	private readonly ILogger _logger;

	private const string BookingCompletedExchangeName = "bookings_completed";
	private const string BookingStartedQueueName = "bookings_started";
	private const string BookingsCompletedQueueName = "bookings_completed";

	public MessageQueueService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<MessageQueueService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;

		var connectionFactory = new ConnectionFactory();
		configuration.GetSection("RabbitMqConnection").Bind(connectionFactory);

		_connection = connectionFactory.CreateConnection();
		_channel = _connection.CreateModel();

		_channel.QueueDeclare(queue: BookingStartedQueueName, durable: true, exclusive: false, autoDelete: false);

		_channel.ExchangeDeclare(BookingCompletedExchangeName, ExchangeType.Direct, true);
		_channel.QueueBind(BookingsCompletedQueueName, BookingCompletedExchangeName, "");
	}

	public void Publish(string message)
	{
		var properties = _channel.CreateBasicProperties();
		properties.Persistent = true;

		var body = Encoding.UTF8.GetBytes(message);

		_channel.BasicPublish(BookingCompletedExchangeName, "", properties, body);
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
				var context = scope.ServiceProvider.GetRequiredService<PostgresContext>();

				var body = args.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				var basketPurchase = JsonConvert.DeserializeObject<BasketPurchase>(message);

				if (basketPurchase == null)
				{
					_channel.BasicReject(args.DeliveryTag, requeue: false);
					return;
				}

				var booking = new Dtos.Booking
				{
					BasketId = basketPurchase.BasketId.ToString(),
					BookingDate = DateTime.UtcNow
				};

				await context.Bookings.AddAsync(booking);

				var movies = context.Movies
					.Where(movie => basketPurchase.Movies.Contains(movie.MovieId))
					.Select(movie => new BookingMovie
					{
						Booking = booking,
						Movie = movie
					});

				await context.BookingMovies.AddRangeAsync(movies);
				await context.SaveChangesAsync();

				_channel.BasicAck(args.DeliveryTag, multiple: false);

				Publish(JsonConvert.SerializeObject(new { basketPurchase.BasketId, CompletedAt = DateTime.UtcNow }));

				_logger.LogInformation($"[x] Processed {message}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing message");

				// Requeue message for retry
				_channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
			}
		};

		_channel.BasicConsume(queue: BookingStartedQueueName, autoAck: false, consumer: consumer);
	}

	public void Dispose()
	{
		_channel.Dispose();
		_connection.Dispose();
	}
}
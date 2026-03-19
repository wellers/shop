using Booking.Dtos;
using Booking.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Booking.Consumers;

public class BookingConsumer(IConnection connection, ILogger<BookingConsumer> logger, IServiceScopeFactory scopeFactory) : BackgroundService, IDisposable
{
	private IModel? _consumerChannel;

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_consumerChannel = connection.CreateModel();

		DeclareTopology(_consumerChannel);
		StartListening();
		
		return Task.CompletedTask;
	}

	public void StartListening()
	{
		_consumerChannel?.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

		var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

		consumer.Received += async (model, args) =>
		{
			try
			{
				var body = args.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				var basketPurchase = JsonConvert.DeserializeObject<BasketPurchase>(message);

				if (basketPurchase == null)
				{
					_consumerChannel?.BasicReject(args.DeliveryTag, requeue: false);
					return;
				}

				var booking = new Dtos.Booking
				{
					BasketId = basketPurchase.BasketId.ToString(),
					BookingDate = DateTime.UtcNow
				};

				using var scope = scopeFactory.CreateScope();
				var context = scope.ServiceProvider.GetRequiredService<PostgresContext>();

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

				Publish(JsonConvert.SerializeObject(new { basketPurchase.BasketId, CompletedAt = DateTime.UtcNow }));

				_consumerChannel?.BasicAck(args.DeliveryTag, multiple: false);

				logger.LogInformation($"[x] Processed {message}");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error processing message");

				// Requeue message for retry
				_consumerChannel?.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
			}
		};

		_consumerChannel.BasicConsume(queue: "bookings_started", autoAck: false, consumer: consumer);
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		if (_consumerChannel is { IsOpen: true })
			_consumerChannel.Close();

		return base.StopAsync(cancellationToken);
	}

	public override void Dispose()
	{
		_consumerChannel?.Dispose();
		base.Dispose();
	}

	private static void DeclareTopology(IModel channel)
	{
		channel.ExchangeDeclare("bookings_started", ExchangeType.Direct, durable: true);

		channel.QueueDeclare(
			queue: "bookings_started",
			durable: true,
			exclusive: false,
			autoDelete: false);

		channel.QueueBind(
			queue: "bookings_started",
			exchange: "bookings_started",
			routingKey: "");
	}

	private void Publish(string message)
	{
		using var channel = connection.CreateModel();

		var properties = channel.CreateBasicProperties();
		properties.Persistent = true;

		var body = Encoding.UTF8.GetBytes(message);

		channel.BasicPublish("bookings_completed", "", properties, body);
	}
}

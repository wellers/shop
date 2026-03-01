using Basket.Messages;
using Basket.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Basket.Consumers;

public class BookingConsumer(IConnection connection, ILogger<BookingConsumer> logger, RedisService redisService) : BackgroundService, IDisposable
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

		var consumer = new EventingBasicConsumer(_consumerChannel);

		consumer.Received += (model, args) =>
		{
			try
			{
				var body = args.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				var bookingCompleted = JsonConvert.DeserializeObject<BookingCompleted>(message);

				if (bookingCompleted == null)
				{
					_consumerChannel?.BasicReject(args.DeliveryTag, requeue: false);
					return;
				}

				redisService.Database?.KeyDelete(bookingCompleted.BasketId.ToString());

				_consumerChannel?.BasicAck(args.DeliveryTag, multiple: false);

				logger.LogInformation($"[x] Deleted basket {bookingCompleted.BasketId}");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error processing message");

				// Requeue message for retry
				_consumerChannel?.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
			}
		};

		_consumerChannel?.BasicConsume(queue: "bookings_completed", autoAck: false, consumer: consumer);
	}

	private static void DeclareTopology(IModel channel)
	{
		channel.ExchangeDeclare("bookings_completed", ExchangeType.Direct, durable: true);

		channel.QueueDeclare(
			queue: "bookings_completed",
			durable: true,
			exclusive: false,
			autoDelete: false);

		channel.QueueBind(
			queue: "bookings_completed",
			exchange: "bookings_completed",
			routingKey: "");
	}		
}

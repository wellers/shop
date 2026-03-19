using Mailout.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mailout.Consumers;

public class BookingConsumer(IConnection connection, ILogger<BookingConsumer> logger) : BackgroundService, IDisposable
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
					return Task.CompletedTask;
				}

				_consumerChannel?.BasicAck(args.DeliveryTag, multiple: false);

				logger.LogInformation($"[x] Email notification received: {bookingCompleted.BasketId}");

				// now Send email
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error processing message");

				// Requeue message for retry
				_consumerChannel?.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
			}

			return Task.CompletedTask;
		};

		_consumerChannel?.BasicConsume(queue: "email_notifications", autoAck: false, consumer: consumer);
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
		channel.ExchangeDeclare("bookings_completed", ExchangeType.Direct, durable: true);

		channel.QueueDeclare(
			queue: "email_notifications",
			durable: true,
			exclusive: false,
			autoDelete: false);

		channel.QueueBind(
			queue: "email_notifications",
			exchange: "bookings_completed",
			routingKey: "");
	}
}
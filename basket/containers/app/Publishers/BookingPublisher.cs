using RabbitMQ.Client;
using System.Text;

namespace Basket.Publishers;

public class BookingPublisher(IConnection connection)
{
	public Task PublishAsync(string message)
	{
		using var channel = connection.CreateModel();

		var properties = channel.CreateBasicProperties();
		properties.Persistent = true;

		var body = Encoding.UTF8.GetBytes(message);

		channel.BasicPublish("bookings_started", "", properties, body);

		return Task.CompletedTask;
	}
}

using RabbitMQ.Client;
using System.Text;

namespace Basket
{
    public class MessageQueueService(IConfigurationRoot configurationRoot)
    {
        private readonly IConfigurationRoot _configurationRoot = configurationRoot;

        public void Publish(string message)
        {
            var connectionFactory = new ConnectionFactory();
            _configurationRoot.GetSection("RabbitMqConnection").Bind(connectionFactory);

            using var connection = connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "bookings", type: "direct", durable: true, autoDelete: false);
            channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue: "bookings", exchange: "bookings", routingKey: "bookings");

            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "bookings", routingKey: "bookings", null, body);            
        }
    }
}

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Basket
{
    public class MessageQueueService(IConfigurationRoot configurationRoot)
    {
        private readonly IConfigurationRoot _configurationRoot = configurationRoot;

        public void ConsumeQueue()
        {
            var connectionFactory = new ConnectionFactory();
            _configurationRoot.GetSection("RabbitMqConnection").Bind(connectionFactory);

            using var connection = connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
           
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine(" [x] Received {0}", message);                
            };

            channel.BasicConsume(queue: "bookings", autoAck: true, consumer: consumer);
        }
    }
}

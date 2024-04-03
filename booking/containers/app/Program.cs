using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .Build();

var app = builder.Build();

var connectionFactory = new ConnectionFactory();
configuration.GetSection("RabbitMqConnection").Bind(connectionFactory);

var connection = connectionFactory.CreateConnection();

using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "bookings", durable: true);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, args) =>
{
    var body = args.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    
    Console.WriteLine(" [x] Received {0}", message);
};

channel.BasicConsume(queue: "booking_queue", autoAck: false, consumer: consumer);

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();
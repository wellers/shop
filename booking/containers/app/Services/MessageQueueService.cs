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

    public MessageQueueService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;

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
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<PostgresContext>();
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

            if (basketPurchase == null)
                return;

            var booking = new Dtos.Booking
            {
                BasketId = basketPurchase.BasketId.ToString(),
                BookingDate = DateTime.UtcNow
            };

            await context.Bookings.AddAsync(booking);

            var movies = context.Movies.Where(movie => basketPurchase.Movies.Contains(movie.MovieId)).Select(movie => new BookingMovie
            {
                Booking = booking,
                Movie = movie
            });

            await context.BookingMovies.AddRangeAsync(movies);

            await context.SaveChangesAsync();

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
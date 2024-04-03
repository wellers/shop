using Newtonsoft.Json;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .Build();

var app = builder.Build();

var redisHostname = configuration.GetValue<string>("RedisHostname");

ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisHostname);
IDatabase db = redis.GetDatabase();

app.MapGet("/add", async (Guid basketId, int movieId) =>
{
    var basket = await db.StringGetAsync(basketId.ToString());

    List<int> movies = [];
    if (basket.HasValue)
    {
        try
        {
            movies = JsonConvert.DeserializeObject<List<int>>(basket.ToString());
        }
        catch 
        { 
            return new { Success = false, Message = "Failed to parse basket from cache." };
        }        
    }

    movies.Add(movieId);

    await db.StringSetAsync(basketId.ToString(), JsonConvert.SerializeObject(movies));

    return new { Success = true, Message = $"{movies.Count} item(s) added to basket." };
});

app.MapGet("/purchase", async (Guid basketId) =>
{
    var basket = await db.StringGetAsync(basketId.ToString());

    var body = Encoding.UTF8.GetBytes(basket.ToString());
    var connectionFactory = new ConnectionFactory();
    configuration.GetSection("RabbitMqConnection").Bind(connectionFactory);

    var connection = connectionFactory.CreateConnection();

    using var channel = connection.CreateModel();

    channel.ExchangeDeclare(exchange: "bookings", type: "direct", durable: true);
    channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false);    
    channel.QueueBind(queue: "bookings", exchange: "bookings", routingKey: "bookings");
    channel.BasicPublish(exchange: "bookings", routingKey: "bookings", null, body);

    channel.Close();
    connection.Close();
});

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();

using Basket;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .Build();

var app = builder.Build();

var redis = new RedisService(configuration);
var db = redis.GetDatabase();

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

    var message = basket.ToString();
    var messageQueue = new MessageQueueService(configuration);
    messageQueue.Publish(message);
});

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();

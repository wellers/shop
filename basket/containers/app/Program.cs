using Basket;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile($"appsettings.json").Build();

builder.Services.AddSingleton<RedisService>();
builder.Services.AddSingleton<MessageQueueService>();

var app = builder.Build();

var redis = app.Services.GetRequiredService<RedisService>();
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

	var success = await db.StringSetAsync(basketId.ToString(), JsonConvert.SerializeObject(movies));

	var message = success
		? $"{movies.Count} item(s) added to basket."
		: "Failed to add item to basket.";

	return new { Success = success, Message = message };
});

app.MapGet("/purchase", async (Guid basketId) =>
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

	var message = new { BasketId = basketId, Movies = movies };

	var rabbitMQService = app.Services.GetRequiredService<MessageQueueService>();
	rabbitMQService.Publish(JsonConvert.SerializeObject(message));

	return new { Success = true, Message = $"Basket '{basketId}' purchased." };
});

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();

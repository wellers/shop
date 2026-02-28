using Basket.Consumers;
using Basket.Publishers;
using Basket.Services;
using RabbitMQ.Client;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json").Build();

var redisHostname = builder.Configuration.GetValue<string>("RedisHostname")
				?? throw new ApplicationException("redisHostname cannot be null.");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisHostname));
builder.Services.AddSingleton<RedisService>();

builder.Services.AddSingleton(sp =>
{
	var factory = new ConnectionFactory();
	builder.Configuration.GetSection("RabbitMqConnection").Bind(factory);
	factory.AutomaticRecoveryEnabled = true;
	return factory.CreateConnection();
});
builder.Services.AddHostedService<BookingConsumer>();
builder.Services.AddSingleton<BookingPublisher>();
builder.Services.AddScoped<BasketService>();

var app = builder.Build();

app.MapGet("/add", async (BasketService basketService, Guid basketId, int movieId) =>
{
	bool success;
	List<int> movies;
	try
	{
		(success, movies) = await basketService.AddMovie(basketId, movieId);
	}
	catch
	{
		return new { Success = false, Message = "Failed to parse basket from cache." };
	}

	var message = success
		? $"{movies.Count} item(s) added to basket."
		: "Failed to add item to basket.";

	return new { Success = success, Message = message };
});

app.MapGet("/purchase", async (BasketService basketService, Guid basketId) =>
{
	bool success;
	try
	{
		success = await basketService.PurchaseBasket(basketId);
	}
	catch
	{
		return new { Success = false, Message = "Failed to parse basket from cache." };
	}

	return new { Success = success, Message = $"Basket '{basketId}' purchased." };
});

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();

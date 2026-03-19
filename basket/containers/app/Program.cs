using Basket.Consumers;
using Basket.Messages;
using Basket.Publishers;
using Basket.Services;
using RabbitMQ.Client;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json").Build();
var startedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

var redisHostname = builder.Configuration.GetValue<string>("RedisHostname")
				?? throw new ApplicationException("redisHostname cannot be null.");

builder.Services
	.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisHostname))
	.AddSingleton<RedisService>()
	.AddSingleton(sp =>
	{
		var factory = new ConnectionFactory();
		builder.Configuration.GetSection("RabbitMqConnection").Bind(factory);
		factory.AutomaticRecoveryEnabled = true;
		factory.DispatchConsumersAsync = true;
		return factory.CreateConnection();
	})
	.AddHostedService<BookingConsumer>()
	.AddSingleton<BookingPublisher>()
	.AddScoped<BasketService>();

var app = builder.Build();

app.MapPost("/baskets/{basketId}/items", async (BasketService basketService, Guid basketId, AddItemRequest request) =>
{
	bool success;
	List<int> movies;
	try
	{
		(success, movies) = await basketService.AddMovie(basketId, request.MovieId);
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

app.MapPost("/baskets/{basketId}/checkout", async (BasketService basketService, Guid basketId) =>
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

app.MapGet("/status", () => Results.Json(new { start = startedAt, now = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();

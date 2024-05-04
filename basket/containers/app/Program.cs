using Basket;
using Basket.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile($"appsettings.json").Build();

builder.Services.AddSingleton<RedisService>();
builder.Services.AddSingleton<MessageQueueService>();
builder.Services.AddSingleton<BasketService>();

var app = builder.Build();

app.MapGet("/add", async (BasketService basketService, Guid basketId, int movieId) =>
{
	var success = false;
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
	var success = false;
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

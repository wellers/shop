using Basket;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .Build();

var app = builder.Build();

var messageQueue = new MessageQueueService(configuration);
messageQueue.ConsumeQueue();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();
using Booking;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"appsettings.json").Build();

builder.Services
	.AddSingleton<MessageQueueService>()
	.AddDbContext<BookingDbContext>();

var app = builder.Build();

var rabbitMQService = app.Services.GetRequiredService<MessageQueueService>();
rabbitMQService.StartListening();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();
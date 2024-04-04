using Booking;
using Booking.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"appsettings.json").Build();

builder.Services
	.AddDbContext<PostgresContext>()
	.AddSingleton<MessageQueueService>();

var app = builder.Build();

var rabbitMQService = app.Services.GetRequiredService<MessageQueueService>();
rabbitMQService.StartListening();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();
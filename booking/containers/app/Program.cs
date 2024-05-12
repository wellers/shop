using Booking.Dtos;
using Booking.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json").Build();

builder.Services
	.AddDbContext<PostgresContext>()
	.AddSingleton<MessageQueueService>()
	.AddGrpc(options =>
	{
		options.EnableDetailedErrors = true;
		options.MaxReceiveMessageSize = null;
	});

var app = builder.Build();

app.MapGrpcService<MoviesGrpcService>().RequireHost("*:5000");

var rabbitMqService = app.Services.GetRequiredService<MessageQueueService>();
rabbitMqService.StartListening();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();
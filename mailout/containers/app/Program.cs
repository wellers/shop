using Mailout.Consumers;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json").Build();

builder.Services
	.AddSingleton(sp =>
	{
		var factory = new ConnectionFactory();
		builder.Configuration.GetSection("RabbitMqConnection").Bind(factory);
		factory.AutomaticRecoveryEnabled = true;
		return factory.CreateConnection();
	})
	.AddHostedService<BookingConsumer>();

var app = builder.Build();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();
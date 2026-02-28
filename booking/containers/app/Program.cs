using Booking.Consumers;
using Booking.Dtos;
using Booking.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json").Build();

builder.Services.AddSingleton(sp =>
{
	var factory = new ConnectionFactory();
	builder.Configuration.GetSection("RabbitMqConnection").Bind(factory);
	factory.AutomaticRecoveryEnabled = true;
	return factory.CreateConnection();
});

builder.Services
	.AddDbContext<PostgresContext>()
	.AddHostedService<BookingConsumer>()
	.AddGrpc(options =>
	{
		options.EnableDetailedErrors = true;
		options.MaxReceiveMessageSize = null;
	});

var app = builder.Build();

app.MapGrpcService<MoviesGrpcService>().RequireHost("*:5000");

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/", () => "🚀 Server ready");

app.Run();
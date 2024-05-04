using Booking.Dtos;
using Booking.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"appsettings.json").Build();

builder.Services
	.AddDbContext<PostgresContext>()
	.AddSingleton<MessageQueueService>();

var app = builder.Build();

var rabbitMqService = app.Services.GetRequiredService<MessageQueueService>();
rabbitMqService.StartListening();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/movies", (PostgresContext context) =>
{
	var movies = context.Movies.Select(movie => new { movie.MovieId, movie.Title, movie.Price }).ToList();

	return Results.Json(new { success = true, movies });
});

app.MapGet("/", () => "🚀 Server ready");

app.Run();
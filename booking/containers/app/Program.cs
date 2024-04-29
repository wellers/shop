using Booking.Dtos;
using Booking.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"appsettings.json").Build();

builder.Services
	.AddDbContext<PostgresContext>()
	.AddSingleton<MessageQueueService>();

var app = builder.Build();

var rabbitMQService = app.Services.GetRequiredService<MessageQueueService>();
rabbitMQService.StartListening();

app.MapGet("/status", () => Results.Json(new { start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }));

app.MapGet("/movies", () =>
{
	using var scope = app.Services.CreateScope();
	var context = scope.ServiceProvider.GetRequiredService<PostgresContext>();

	var movies = context.Movies.Select(movie => new { movie.MovieId, movie.Title, movie.Price }).ToList();

	return Results.Json(new { success = true, movies });
});

app.MapGet("/", () => "🚀 Server ready");

app.Run();
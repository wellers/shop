using Catalog.Database;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Movies;

namespace Catalog.Jobs
{
	public class SyncBookableMovies(IServiceProvider serviceProvider, IConfiguration configuration, BackgroundJobOptions options) : IBackgroundJob
	{
		public string? Name { get; } = options.Name;

		public string? CronSchedule { get; } = options.CronPattern;

		public string? CronTimeZone { get; } = null;

		public bool RunImmediately { get; } = false;

		public async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("Sync Bookable Movies job started.");
			
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<MongoContext>();

			var getMoviesUrl = configuration.GetValue<string>("GetMoviesUrl");

			if (getMoviesUrl is null)
			{
				Console.WriteLine("GetMoviesUrl is not set.");
				return;
			}

			var allowInsecureGrpc = configuration.GetValue<bool>("AllowInsecureGrpc");
			HttpClientHandler? httpHandler = null;

			if (allowInsecureGrpc)
			{
				httpHandler = new HttpClientHandler
				{
					ServerCertificateCustomValidationCallback =
						HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
				};
				AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
			}

			using var channel = GrpcChannel.ForAddress(getMoviesUrl, new GrpcChannelOptions { HttpHandler = httpHandler });
			var client = new MovieService.MovieServiceClient(channel);

			var response = await client.GetMoviesAsync(new GetMoviesRequest(), cancellationToken: cancellationToken);

			if (response == null)
			{
				Console.WriteLine("Unable to retrieve Movies from Booking service.");
				return;
			}

			var moviesById = response.Movies.ToDictionary(movie => movie.Id, movie => new Models.Movie
			{
				Id = movie.Id,
				Title = movie.Title,
				Price = Convert.ToDecimal(movie.Price)
			});
			
			var existingMovies = await context.Movies.ToListAsync(cancellationToken: cancellationToken);

			var processedMovieIds = new List<int>();
			var hasChanges = false;
			foreach (var movie in existingMovies)
			{
				if (moviesById.TryGetValue(movie.Id, out var toUpdate))
				{
					movie.Title = toUpdate.Title;
					movie.Price = toUpdate.Price;
					processedMovieIds.Add(movie.Id);
					hasChanges = true;
				}
				else
				{
					context.Movies.Remove(movie);
					hasChanges = true;
					continue;
				}
			}

			var toAdd = moviesById
				.Where(kvp => !processedMovieIds.Contains(kvp.Key))
				.Select(kvp => kvp.Value)
				.ToList();
			
			if (toAdd.Any())
			{
				await context.Movies.AddRangeAsync(toAdd, cancellationToken);
				hasChanges = true;
			}

			if (hasChanges)
			{
				await context.SaveChangesAsync(cancellationToken);
			}

			Console.WriteLine("Sync Bookable Movies job completed.");			
		}
	}
}

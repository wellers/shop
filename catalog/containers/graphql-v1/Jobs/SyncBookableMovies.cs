using Catalog.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Jobs
{
	public class SyncBookableMovies(IServiceProvider serviceProvider, IConfiguration configuration, BackgroundJobOptions options) : IBackgroundJob
	{
		public string Name { get; } = options.Name;

		public string CronSchedule { get; } = options.CronPattern;

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

			var response = await Utils.HttpRequestHelper.Get<Dtos.GetMoviesApiResponse>(getMoviesUrl);

			if (!response.Success)
			{
				Console.WriteLine("Unable to retrieve Movies from Booking service.");
				return;
			}

			var moviesById = response.Movies.ToDictionary(movie => movie.MovieId, movie => new Models.Movie
			{
				Id = movie.MovieId,
				Title = movie.Title,
				Price = movie.Price.Value
			});
			
			var existingMovies = await context.Movies.ToListAsync(cancellationToken: cancellationToken);

			var processedMovieIds = new List<int>();
			foreach (var movie in existingMovies)
			{
				if (moviesById.TryGetValue(movie.Id, out var toUpdate))
				{
					movie.Title = toUpdate.Title;
					movie.Price = toUpdate.Price;
					processedMovieIds.Add(movie.Id);
					context.SaveChanges();
				}
				else
				{
					var toRemove = await context.Movies.SingleAsync(m => m.Id == movie.Id, cancellationToken: cancellationToken);
					context.Movies.Remove(toRemove);
					context.SaveChanges();
					continue;
				}
			}

			var toAdd = moviesById.Where(kvp => !processedMovieIds.Contains(kvp.Key)).Select(kvp => kvp.Value);
			if (toAdd.Any())
			{
				await context.Movies.AddRangeAsync(toAdd, cancellationToken);
				context.SaveChanges();
			}

			Console.WriteLine("Sync Bookable Movies job completed.");			
		}
	}
}

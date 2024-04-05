using Catalog.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Models
{
	public sealed class Query
	{
		[GraphQLName("find_movies")]
		public async Task<MoviesFindResult> GetMovies([Service] MongoContext context, MoviesFindFilter? filter = null)
		{
			IQueryable<Movie> movies = context.Movies;
			
			if (filter != null)
			{
				movies = movies.Where(movie =>
					(!filter.Id.HasValue || movie.Id == filter.Id)
					&& (filter.Title == null || movie.Title == filter.Title)
				);
			}

			var docs = await movies.ToListAsync();

			return new MoviesFindResult
			{
				Success = true,
				Message = "Matching records.",
				Docs = docs
			};
		}
	}
}

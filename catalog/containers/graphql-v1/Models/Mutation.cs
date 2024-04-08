using Catalog.Database;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Models
{
	public sealed class Mutation
	{
		[GraphQLName("insert_movie")]
		public async Task<MovieInsertResult> AddMovie([Service] MongoContext context, MovieInsertInput movie)
		{
			var result = context.Movies.Add(new Movie
			{
				Title = movie.Title,
				Price = movie.Price,
			});

			await context.SaveChangesAsync();

			return new MovieInsertResult
			{
				Success = result.IsKeySet,
				Message = "1 record inserted."
			};
		}

		[GraphQLName("remove_movies")]
		public async Task<MoviesRemoveResult> RemoveMovies([Service] MongoContext context, MoviesRemoveInput movie)
		{
			var toRemove = await context.Movies.Where(m => movie.Ids.Contains(m.Id)).ToListAsync();

			context.Movies.RemoveRange(toRemove);

			await context.SaveChangesAsync();

			return new MoviesRemoveResult
			{
				Success = true,
				Message = $"{toRemove.Count} record(s) removed."
			};
		}
	}
}

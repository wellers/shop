using Booking.Dtos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Movies;

namespace Booking.Services;

public class MoviesGrpcService(PostgresContext dbContext) : MovieService.MovieServiceBase
{
	public override async Task<GetMoviesResponse> GetMovies(GetMoviesRequest request, ServerCallContext context)
	{
		var movies = await dbContext.Movies.ToListAsync();

		var response = new GetMoviesResponse();
		response.Movies.AddRange(movies.Select(movie => new Movies.Movie
		{
			Id = movie.MovieId,
			Price = movie.Price.HasValue ? Convert.ToSingle(movie.Price.Value) : default,
			Title = movie.Title
		}));

		return response;
	}
}
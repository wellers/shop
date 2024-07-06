using Booking.Dtos;
using Grpc.Core;
using Movies;

namespace Booking.Services;

public class MoviesGrpcService(PostgresContext dbContext) : MovieService.MovieServiceBase
{
	public override Task<GetMoviesResponse> GetMovies(GetMoviesRequest request, ServerCallContext context)
	{
		var movies = dbContext.Movies.ToList();

		var response = new GetMoviesResponse();
		response.Movies.AddRange(movies.Select(movie => new Movies.Movie
		{
			Id = movie.MovieId,
			Price = movie.Price.HasValue ? Convert.ToSingle(movie.Price.Value) : default,
			Title = movie.Title
		}));

		return Task.FromResult(response);
	}
}
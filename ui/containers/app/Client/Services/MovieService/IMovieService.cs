using System;
using Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Services.MovieService
{
	public interface IMovieService
	{
		List<Movie> Movies { get; set; }
		string Message { get; set; }
		Task GetMovies();
		Task AddMovie(Guid basketId, int id);
		Task PurchaseBasket(Guid basketId);
	}
}

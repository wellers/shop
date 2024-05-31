using Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Client.Services.MovieService
{
	public class MovieService : IMovieService
	{
		private readonly HttpClient _http;
		private readonly NavigationManager _navigationManager;

		public MovieService(HttpClient http, NavigationManager navigationManager)
		{
			_http = http;
			_navigationManager = navigationManager;
		}

		public List<Movie> Movies { get; set; } = new List<Movie>();
		public string Message { get; set; } = string.Empty;

		public async Task AddMovie(Guid basketId, int id)
		{
			var response = await _http.PostAsJsonAsync("api/movie/add", new { basketId, id });
			Message = response.IsSuccessStatusCode ? $"Movie id: {id} added to basket." : "An error has occurred.";
		}

		public async Task PurchaseBasket(Guid basketId)
		{
			var response = await _http.PostAsJsonAsync("api/movie/purchase", new { basketId });
			Message = response.IsSuccessStatusCode ? $"Basket {basketId} purchased." : "An error has occurred.";
		}

		public async Task GetMovies()
		{
			var result = await _http.GetFromJsonAsync<List<Movie>>("api/movie");
			if (result != null)
				Movies = result;
		}
	}
}

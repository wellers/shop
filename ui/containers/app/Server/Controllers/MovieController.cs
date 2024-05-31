using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Server.Dtos;
using Server.Models;
using Server.Utils;
using Movie = Shared.Movie;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MovieController : ControllerBase
	{
		[HttpGet]
		public async Task<ActionResult<List<Movie>>> GetMovies()
		{
			var client = new GraphQLHttpClient("http://catalog.io/graphql", new NewtonsoftJsonSerializer());

			var request = new GraphQLHttpRequest
			{
				Query = @"
				        query ($filter: MoviesFindFilterInput) {
						  find_movies(filter: $filter) {
						    success,
						    message,
						    docs {
						      id,
						      title,
						      price   
						    } 
						  }
						}
				        ",
				Variables = new
				{
					filter = new {}
				}
			};

			var response = await client.SendQueryAsync<ResponseType>(request);
			
			return Ok(response.Data.FindMovies.Docs.Select(movie => new Movie
			{
				Id = movie.Id,
				Price = movie.Price,
				Title = movie.Title
			}));
		}

		[HttpPost("add")]
		public async Task<ActionResult<bool>> AddMovie(AddMovieModel model)
		{
			var response = await HttpRequestHelper.Get<ApiResponse>($"http://basket.io/add?basketId={model.BasketId}&movieId={model.Id}");

			return Ok(response.Success);
		}
		
		[HttpPost("purchase")]
		public async Task<ActionResult<bool>> PurchaseBasket(PurchaseMovieModel model)
		{
			var response = await HttpRequestHelper.Get<ApiResponse>($"http://basket.io/purchase?basketId={model.BasketId}");

			return Ok(response.Success);
		}
	}
}

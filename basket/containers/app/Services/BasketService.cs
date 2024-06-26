using Newtonsoft.Json;

namespace Basket.Services
{
	public class BasketService(RedisService redisService, MessageQueueService messageQueueService)
	{
		public async Task<(bool, List<int>)> AddMovie(Guid basketId, int movieId)
		{
			var database = redisService.GetDatabase();

			var basket = await database.StringGetAsync(basketId.ToString());

			List<int> movies = [];
			if (basket.HasValue)
				movies = JsonConvert.DeserializeObject<List<int>>(basket.ToString()) ?? [];

			movies.Add(movieId);

			var success = await database.StringSetAsync(basketId.ToString(), JsonConvert.SerializeObject(movies));

			return (success, movies);
		}

		public async Task<bool> PurchaseBasket(Guid basketId)
		{
			var database = redisService.GetDatabase();

			var basket = await database.StringGetAsync(basketId.ToString());

			List<int> movies = [];
			if (basket.HasValue)
				movies = JsonConvert.DeserializeObject<List<int>>(basket.ToString());

			var message = new { BasketId = basketId, Movies = movies };

			messageQueueService.Publish(JsonConvert.SerializeObject(message));

			return true;
		}
	}
}
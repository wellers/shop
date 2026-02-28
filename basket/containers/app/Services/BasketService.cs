using Basket.Publishers;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Basket.Services
{
	public class BasketService(RedisService redisService, BookingPublisher bookingPublisher)
	{
		private readonly IDatabase _database = redisService.Database ?? throw new ApplicationException("Redis database is not available.");

		public async Task<(bool, List<int>)> AddMovie(Guid basketId, int movieId)
		{
			var basket = await _database.StringGetAsync(basketId.ToString());

			List<int> movies = [];
			if (basket.HasValue)
				movies = JsonConvert.DeserializeObject<List<int>>(basket.ToString()) ?? [];

			movies.Add(movieId);

			var success = await _database.StringSetAsync(basketId.ToString(), JsonConvert.SerializeObject(movies));

			return (success, movies);
		}

		public async Task<bool> PurchaseBasket(Guid basketId)
		{
			var basket = await _database.StringGetAsync(basketId.ToString());

			List<int> movies = [];
			if (basket.HasValue)
				movies = JsonConvert.DeserializeObject<List<int>>(basket.ToString());

			var message = new { BasketId = basketId, Movies = movies };

			await bookingPublisher.PublishAsync(JsonConvert.SerializeObject(message));

			return true;
		}
	}
}
using Newtonsoft.Json;

namespace Basket
{
	public class BasketService(RedisService redisService, MessageQueueService messageQueueService)
	{
		private readonly RedisService _redisService = redisService;
		private readonly MessageQueueService _messageQueueService = messageQueueService;

		public async Task<(bool, List<int>)> AddMovie(Guid basketId, int movieId)
		{
			var database = _redisService.GetDatabase();

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
			var database = _redisService.GetDatabase();

			var basket = await database.StringGetAsync(basketId.ToString());

			List<int> movies = [];
			if (basket.HasValue)
				movies = JsonConvert.DeserializeObject<List<int>>(basket.ToString());

			var message = new { BasketId = basketId, Movies = movies };

			_messageQueueService.Publish(JsonConvert.SerializeObject(message));

			return true;
		}
	}
}
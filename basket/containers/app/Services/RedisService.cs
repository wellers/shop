using StackExchange.Redis;

namespace Basket.Services
{
	public class RedisService(IConfiguration configuration)
	{
		public IDatabase GetDatabase()
		{
			var redisHostname = configuration.GetValue<string>("RedisHostname") 
				?? throw new ApplicationException("redisHostname cannot be null.");

			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisHostname);
			return redis.GetDatabase();
		}
	}
}

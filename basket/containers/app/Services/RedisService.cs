using StackExchange.Redis;

namespace Basket.Services
{
	public class RedisService(IConnectionMultiplexer connectionMultiplexer)
	{
		public IDatabase? Database { get; private set; } = connectionMultiplexer.GetDatabase();
	}
}

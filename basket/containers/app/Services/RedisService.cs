using StackExchange.Redis;

namespace Basket.Services
{
	public class RedisService
	{
		public IDatabase? Database { get; private set; }

		public RedisService(IConfiguration configuration)
		{
			var redisHostname = configuration.GetValue<string>("RedisHostname") 
				?? throw new ApplicationException("redisHostname cannot be null.");
			
			var connection = ConnectionMultiplexer.Connect(redisHostname);
			Database = connection.GetDatabase();
		}		
	}
}

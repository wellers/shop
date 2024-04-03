using StackExchange.Redis;

namespace Basket
{
    public class RedisService(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        public IDatabase GetDatabase()
        {
            var redisHostname = _configuration.GetValue<string>("RedisHostname");

            if (redisHostname == null)
                throw new ApplicationException("redisHostname cannot be null.");

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisHostname);
            return redis.GetDatabase();
        }
    }
}

using StackExchange.Redis;

namespace Basket
{
    public class RedisService(IConfigurationRoot configurationRoot)
    {
        private readonly IConfigurationRoot _configurationRoot = configurationRoot;

        public IDatabase GetDatabase()
        {
            var redisHostname = _configurationRoot.GetValue<string>("RedisHostname");

            if (redisHostname == null)
                throw new ApplicationException("redisHostname cannot be null.");

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisHostname);
            return redis.GetDatabase();
        }
    }
}

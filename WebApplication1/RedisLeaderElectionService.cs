namespace WebApplication1;

using StackExchange.Redis;

public class RedisLeaderElectionService(string redisConnectionString, string instanceId)
{
    private readonly IDatabase _redis = ConnectionMultiplexer.Connect(redisConnectionString).GetDatabase();
    private readonly string _instanceId = instanceId;
    private readonly string _lockKey = "leader-lock";
    private readonly TimeSpan _lockExpiry = TimeSpan.FromSeconds(10);

    public async Task<bool> TryBecomeLeaderAsync()
    {
        return await _redis.StringSetAsync(
            key: _lockKey,
            value: _instanceId,
            expiry: _lockExpiry,
            when: When.NotExists
        );
    }

    public async Task<string?> GetCurrentLeaderAsync()
    {
        return await _redis.StringGetAsync(_lockKey);
    }

    public async Task<bool> RenewLeadershipAsync()
    {
        var currentLeader = await _redis.StringGetAsync(_lockKey);
        if (currentLeader == _instanceId)
        {
            return await _redis.StringSetAsync(_lockKey, _instanceId, _lockExpiry);
        }

        return false;
    }
}


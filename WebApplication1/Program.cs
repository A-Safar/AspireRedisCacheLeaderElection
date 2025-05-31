using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

// Redis configuration via Aspire
builder.AddRedisDistributedCache("redis");

var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID") ?? "default";

// Register RedisLeaderElectionService as a singleton
builder.Services.AddSingleton<RedisLeaderElectionService>(sp =>
{
    var redisConnectionString = builder.Configuration["ConnectionStrings:redis"];
    return new RedisLeaderElectionService(redisConnectionString, instanceId);
});

var app = builder.Build();

app.MapGet("/", async (RedisLeaderElectionService leaderElection) =>
{
    // check if i'm the leader
    var currLeader = await leaderElection.GetCurrentLeaderAsync();
    if (currLeader == instanceId)
    {
        // renew leadership
        var renewed = await leaderElection.RenewLeadershipAsync();
        if (renewed)
            return $"{instanceId} is the LEADER, leadership renewed.";
        else
            return $"{instanceId} is the LEADER, but failed to renew leadership.";
    }
    var isLeader = await leaderElection.TryBecomeLeaderAsync();
    return isLeader
        ? $"{instanceId} is now the LEADER"
        : $"{instanceId} is a FOLLOWER. Current leader: {await leaderElection.GetCurrentLeaderAsync()}";
});

app.Run();

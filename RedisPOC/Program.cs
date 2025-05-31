var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithRedisCommander();

// Add MyService twice with different IDs and ports
builder.AddProject<Projects.WebApplication1>("myservice-1")
    .WaitFor(redis)
       .WithEnvironment("INSTANCE_ID", "myservice-1")
       .WithReference(redis)
       .WithHttpEndpoint(port: 5001);

builder.AddProject<Projects.WebApplication1>("myservice-2")
    .WaitFor(redis)
       .WithEnvironment("INSTANCE_ID", "myservice-2")
       .WithReference(redis)
       .WithHttpEndpoint(port: 5002);

builder.Build().Run();

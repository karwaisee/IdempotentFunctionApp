# Idempotence Function App
One of the key properties for Function App (serverless)  best practice is **Idempotence**
> Idempotence => even if the same event being triggered more than once, the result will be the same; i.e. the extra trigger will result in no-op.

To achieve this, a _Cache_ is used to register each event to ensure event with the same event ID will not be processed. As serverless setup could involve multiple server instances, a **Distributed Cache** is required to ensure uniqueness across multiple instances.

## How to ensure function idempotent
1. Setup distributed cache via Dependency Injection, use Redis as IDistributedCache
```nuget
Install-Package Microsoft.Extensions.Caching.StackExchangeRedis
```
```C#
public override void Configure(IFunctionsHostBuilder builder)
{
    builder
        .Services
        .AddStackExchangeRedisCache(o =>
        {
            o.Configuration = "localhost:6379";
        });
}
```
2. On the function trigger, upon receiving the event, determine the unique ID of the event
```C#
public async Task ProcessEventAsync(EventData eventData)
{
    string eventId = $"{eventData.SystemProperties.PartitionKey}_{eventData.SystemProperties.Offset}";
    ...
}
```

3. Use IDistributedCache (Redis) to check if unique ID exists, if exists, then do not proceed further
```C#
public async Task ProcessEventAsync(EventData eventData)
{
    ...
    IDistributedCache cache;
    var cacheResult = await cache.GetAsync(eventId).ConfigureAwait(false);
    if (cacheResult != null)
    {
        log.LogInformation($"Skipped previously processed event {eventId}");
        return;
    }
    ...
}
```
4. If unique ID does not exists, then mark the unique ID in IDistributedCache and process the event
```C#
public async Task ProcessEventAsync(EventData eventData)
{
    ...
    var cacheValue = DateTime.UtcNow.ToBinary().ToString();
    await cache
        .SetAsync(eventId, Encoding.UTF8.GetBytes(cacheValue), cacheEntryOptions)
        .ConfigureAwait(false);
    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array);

    // TODO: Add your processing logic.

    log.LogInformation($"Processed event {eventId}: {messageBody}");
}
```
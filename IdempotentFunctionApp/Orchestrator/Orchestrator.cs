using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text;
using System;
using Microsoft.Extensions.Caching.Distributed;

namespace IdempotentFunctionApp
{
    public class Orchestrator : IOrchestrator
    {
        private readonly IDistributedCache cache;
        private readonly ILogger log;
        private readonly DistributedCacheEntryOptions cacheEntryOptions;

        public Orchestrator(IDistributedCache cache, ILogger<Orchestrator> log)
        {
            this.cache = cache;
            this.log = log;
            cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));
        }

        public async Task ProcessEventAsync(EventData eventData)
        {
            // Alternatively, can use partitionkey+sequencenumber
            string eventId = $"{eventData.SystemProperties.PartitionKey}_{eventData.SystemProperties.Offset}";
            var cacheResult = await cache.GetAsync(eventId).ConfigureAwait(false);
            if (cacheResult != null)
            {
                log.LogInformation($"Skipped previously processed event {eventId}");
                return;
            }

            var cacheValue = DateTime.UtcNow.ToBinary().ToString();
            await cache
                .SetAsync(eventId, Encoding.UTF8.GetBytes(cacheValue), cacheEntryOptions)
                .ConfigureAwait(false);
            string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

            // TODO: Add your processing logic.

            log.LogInformation($"Processed event {eventId}: {messageBody}");
        }
    }
}

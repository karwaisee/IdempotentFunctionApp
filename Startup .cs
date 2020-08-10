using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace IdempotentFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Registering services
            builder
                .Services
                .AddSingleton<IOrchestrator, Orchestrator>();

            builder
                .Services
                .AddStackExchangeRedisCache(o =>
                {
                    o.Configuration = "localhost:6379";
                });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace IdempotentFunctionApp
{
    public class Function1
    {
        private readonly IOrchestrator orchestrator;

        public Function1(IOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        [FunctionName("Function1")]
        public async Task Run(
            [EventHubTrigger(
                eventHubName:"eventhub-name",
                Connection = "evhConnStr",
                ConsumerGroup = "myconsumergroup")]
            EventData[] events)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    await orchestrator.ProcessEventAsync(eventData).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.
            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}

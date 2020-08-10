using Microsoft.Azure.EventHubs;
using System.Threading.Tasks;

namespace IdempotentFunctionApp
{
    public interface IOrchestrator
    {
        Task ProcessEventAsync(EventData eventData);
    }
}

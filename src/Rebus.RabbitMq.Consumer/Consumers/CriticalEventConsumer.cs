using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.RabbitMq.Common;

namespace Rebus.RabbitMq.Consumer.Consumers
{
    public class CriticalEventConsumer : IHandleMessages<CriticalEvent>
    {
        private readonly ILogger<CriticalEventConsumer> _logger;

        public CriticalEventConsumer(ILogger<CriticalEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Handle(CriticalEvent criticalEvent)
        {
            _logger.LogInformation("CriticalEvent received: {Id} - {Description}", criticalEvent.Id, criticalEvent.Description);

            return Task.CompletedTask;
        }
    }
}

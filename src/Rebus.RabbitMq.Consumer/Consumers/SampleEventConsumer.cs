using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.RabbitMq.Common;

namespace Rebus.RabbitMq.Consumer.Consumers
{
    public class SampleEventConsumer : IHandleMessages<SampleEvent>
    {
        private readonly ILogger<SampleEventConsumer> _logger;

        public SampleEventConsumer(ILogger<SampleEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Handle(SampleEvent sampleEvent)
        {
            _logger.LogInformation("SampleEvent received: {Id} - {Description}", sampleEvent.Id, sampleEvent.Description);

            return Task.CompletedTask;
        }
    }
}

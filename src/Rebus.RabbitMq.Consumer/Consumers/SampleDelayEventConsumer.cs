using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Pipeline;
using Rebus.RabbitMq.Common;

namespace Rebus.RabbitMq.Consumer.Consumers
{
    public class SampleDelayEventConsumer : IHandleMessages<SampleDelayEvent>
    {
        private readonly ILogger<SampleDelayEventConsumer> _logger;

        public SampleDelayEventConsumer(ILogger<SampleDelayEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Handle(SampleDelayEvent sampleDelayEvent)
        {
            var context = MessageContext.Current;

            //_logger.LogInformation("SampleEvent received: {Id} - {Description} - Attempt {Attempt} + {Redelivery}", sampleDelayEvent.Id, sampleDelayEvent.Description, context.GetRetryAttempt(), context.GetRedeliveryCount());

            _logger.LogInformation("SampleEvent received: {Id} - {Description} - Attempt {Attempt} + {Redelivery}", sampleDelayEvent.Id, sampleDelayEvent.Description, 1, 1);

            throw new Exception("Simulated exception to demonstrate retry mechanism.");
        }
    }
}

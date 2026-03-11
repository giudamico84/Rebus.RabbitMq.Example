using Microsoft.Extensions.Logging;
using Rebus.Bus;
using System.CommandLine;

namespace Rebus.RabbitMq.Publisher.Commands
{
    public class PublishSampleEventCommand : Command
    {
        private readonly IBus _bus;
        private readonly ILogger<PublishSampleEventCommand> _logger;

        public PublishSampleEventCommand(IBus bus, ILogger<PublishSampleEventCommand> logger) : base("publish-sample-event", "Publishes a SampleEvent to RabbitMQ")
        {
            _bus = bus;
            _logger = logger;
                        
            this.Options.Add(new Option<int>("--number_event", "-num") { Description = "Number event to send", DefaultValueFactory = _ => 1 });

            this.SetAction(SetHandler);            
        }

        private async Task SetHandler(ParseResult parseResult)
        {
            for (int i = 0; i < parseResult.GetValue<int>("--number_event"); i++)
            {
                var message = new Common.SampleEvent
                {
                    Id = Guid.NewGuid(),
                    Description = $"Hello RabbitMQ {i}"
                };

                await _bus.Publish(message);
                _logger.LogInformation("Published SampleEvent {Id} with description '{Description}'", message.Id, message.Description);

                await Task.Delay(TimeSpan.FromSeconds(1)); // Simulate some delay between messages
            }
        }        
    }
}

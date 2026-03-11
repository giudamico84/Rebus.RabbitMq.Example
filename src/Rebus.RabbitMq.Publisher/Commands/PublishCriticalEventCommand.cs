using Microsoft.Extensions.Logging;
using Rebus.Bus;
using System.CommandLine;

namespace Rebus.RabbitMq.Publisher.Commands
{
    public class PublishCriticalEventCommand : Command
    {
        private readonly IBus _bus;
        private readonly ILogger<PublishCriticalEventCommand> _logger;

        public PublishCriticalEventCommand(IBus bus, ILogger<PublishCriticalEventCommand> logger) : base("publish-critical-event", "Publishes a CriticalEvent to RabbitMQ")
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
                var message = new Common.CriticalEvent
                {
                    Id = Guid.NewGuid(),
                    Description = $"Hello RabbitMQ {i}"
                };

                await _bus.Send(message);
                _logger.LogInformation("Send CriticalEvent {Id} with description '{Description}'", message.Id, message.Description);

                await Task.Delay(TimeSpan.FromSeconds(1)); // Simulate some delay between messages
            }
        }
    }
}

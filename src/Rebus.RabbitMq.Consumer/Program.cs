using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using Rebus.Config;
using Rebus.RabbitMq.Consumer.Consumers;
using Rebus.Bus;
using Rebus.RabbitMq.Common;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;

// Configure NLog programmatically to log to console and file
var nlogConfig = new LoggingConfiguration();

var consoleTarget = new ConsoleTarget("console")
{
    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${when:when='${scopeproperty:Arguments}'!='':inner=Arguments=${scopeproperty:Arguments}} ${exception:format=ToString}"
};

var fileTarget = new FileTarget("file")
{
    FileName = "${basedir}/logs/app.log",
    ArchiveFileName = "${basedir}/logs/archives/app.{#}.log",
    MaxArchiveFiles = 7,
    ArchiveAboveSize = 10485760, // 10 MB
    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${when:when='${scopeproperty:Arguments}'!='':inner=Arguments=${scopeproperty:Arguments}} ${exception:format=ToString}"
};

nlogConfig.AddTarget(consoleTarget);
nlogConfig.AddTarget(fileTarget);
nlogConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
nlogConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget);

LogManager.Configuration = nlogConfig;

// Setup configuration with fake RabbitMQ settings
var inMemorySettings = new Dictionary<string, string?>
{
    ["RabbitMQ:ConnectionString"] = "amqp://guest:guest@localhost:5672/TEST"
};

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemorySettings)
    .Build();

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true,
                IncludeScopes = true
            });
        });

        services.AddRebus(cfg => cfg
            .Transport(t => t.UseRabbitMq(configuration.GetSection("RabbitMQ:ConnectionString").Value, "rebus-rabbitmq"))
            .Routing(r => r.TypeBased().Map<CriticalEvent>("critical-event-exchange"))
            .Options(o => o.RetryStrategy(maxDeliveryAttempts: 5)));

        services.AutoRegisterHandlersFromAssemblyOf<SampleEventConsumer>();
        services.AutoRegisterHandlersFromAssemblyOf<SampleDelayEventConsumer>();
        services.AutoRegisterHandlersFromAssemblyOf<CriticalEventConsumer>();
    })
    .Build();

var bus = host.Services.GetRequiredService<IBus>();

await bus.Subscribe<SampleEvent>();
await bus.Subscribe<SampleDelayEvent>();
await bus.Subscribe<CriticalEvent>();

await host.RunAsync();

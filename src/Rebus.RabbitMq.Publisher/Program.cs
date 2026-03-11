using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Rebus.RabbitMq.Publisher.Commands;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.RabbitMq.Common;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
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
        var inMemorySettings = new Dictionary<string, string>
        {
            ["RabbitMQ:ConnectionString"] = "amqp://guest:guest@localhost:5672/TEST"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();


        using var host = Host.CreateDefaultBuilder()
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
                    .Transport(t => t.UseRabbitMqAsOneWayClient(configuration.GetSection("RabbitMQ:ConnectionString").Value))
                    .Routing(r => r.TypeBased().Map<CriticalEvent>("critical-event-queue")));

                services.AddTransient<PublishSampleEventCommand>();
                services.AddTransient<PublishSampleDelayEventCommand>();
                services.AddTransient<PublishCriticalEventCommand>();
            }).Build();

        var rootCommand = new RootCommand("Rebus RabbitMQ Publisher")
        {
            host.Services.GetRequiredService<PublishSampleEventCommand>(),
            host.Services.GetRequiredService<PublishSampleDelayEventCommand>(),
            host.Services.GetRequiredService<PublishCriticalEventCommand>()
        };

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttLoggerLite;

Console.WriteLine("Starting up..");

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        services.AddHostedService<MqttWorker>();
    })
    .Build();

await host.RunAsync();

Console.WriteLine("The end..");

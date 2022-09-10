using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttLoggerLite;

Console.WriteLine("** Starting up..");

var cts = new CancellationTokenSource();

// clean up when stopped by systemd
AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    Console.WriteLine("** Process SIGTERM..");
    cts.Cancel();
};


IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd() // runnin on Linux with systemd
    .ConfigureServices(services =>
    {
        services.AddHostedService<MqttWorker>();
    })
    .Build();

await host.RunAsync();

Console.WriteLine("** The end..");
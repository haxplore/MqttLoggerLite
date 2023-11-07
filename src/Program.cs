using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MqttLoggerLite;
using System;

Console.WriteLine("** Starting up..");
Console.WriteLine("Configuration file env variable: MQTTLOGGER_CONFIG");

var cts = new CancellationTokenSource();

// clean up when stopped by systemd
AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    Console.WriteLine("** Process SIGTERM..");
    cts.Cancel();
};

var configFile = Environment.GetEnvironmentVariable("MQTTLOGGER_CONFIG");

WorkerSettings ws = ConfigHelper.LoadData(configFile);

if (!ws.Validate())
{
    Console.WriteLine("** Environment variables missing");
    Environment.Exit(1);
}

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd() // running on Linux with systemd
    .ConfigureServices(services =>
    {
        services.AddHostedService<MqttWorker>(s => new MqttWorker(s.GetRequiredService<ILogger<MqttWorker>>(), ws));
    })
    .Build();

await host.RunAsync();

Console.WriteLine("** The end..");
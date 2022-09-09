using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using LiteDB;

namespace MqttLoggerLite
{
    public class MqttWorker : BackgroundService
    {
        private readonly ILogger<MqttWorker> _logger;
        IMqttClient? _mqttClient;
        WorkerSettings _workerSettings;

        public MqttWorker(ILogger<MqttWorker> logger)
        {
            _logger = logger;

            _workerSettings = new()
            {
                MqttServer = Environment.GetEnvironmentVariable("MQTT_SERVER"),
                MqttTopic = Environment.GetEnvironmentVariable("MQTT_TOPIC"),
                LiteDbFile = Environment.GetEnvironmentVariable("LITEDB_FILE")
            };

            if (!_workerSettings.Validate())
            {
                Console.WriteLine("Environment variables missing");
                Environment.Exit(1);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_workerSettings.MqttServer)
                .Build();

            // Setup message handling before connecting so that queued messages are also handled properly.
            // When there is no event handler attached all received messages get lost.
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;

            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => { f.WithTopic(_workerSettings.MqttTopic); })
                .Build();

            await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            _logger.LogInformation($"MQTT client subscribed to topic {_workerSettings.MqttTopic}.");
        }

        private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogInformation("Received application message.");

            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            var obj = System.Text.Json.JsonSerializer.Deserialize<TasmotaSensor>(payload);
            WriteObject(obj);
            _logger.LogInformation(payload);

            return Task.CompletedTask;
        }

        private void WriteObject(TasmotaSensor data)
        {
            using (var db = new LiteDatabase(_workerSettings.LiteDbFile))
            {
                var col = db.GetCollection<TasmotaSensor>("tasmota");
                col.Insert(data);
                col.EnsureIndex(x => x.Time);
            }
        }
    }
}

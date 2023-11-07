using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet;
using System.Text;
using System.Text.Json;
using MongoDB.Driver;

namespace MqttLoggerLite
{
    public class MqttWorker : BackgroundService, IDisposable
    {
        private readonly ILogger<MqttWorker> _logger;
        IMqttClient? _mqttClient;
        
        WorkerSettings _workerSettings;
        public WorkerSettings settings { get { return _workerSettings; } set { _workerSettings = value; } }

        public MqttWorker(ILogger<MqttWorker> logger, WorkerSettings ws)
        {
            _logger = logger;
            _workerSettings = ws;
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
            var obj = JsonSerializer.Deserialize<TasmotaSensor>(payload);
            
            if (obj != null)
            {
                SaveObject(obj);
                _logger.LogInformation(payload);
            }

            return Task.CompletedTask;
        }

        private void SaveObject(TasmotaSensor data)
        {
            // add the latest reading
            var client = new MongoClient(_workerSettings.MongoDbServer);

            var collection = client.GetDatabase(_workerSettings.MongoDbCollection).GetCollection<TasmotaSensor>("tasmota");

            collection.InsertOne(data);
        }

        #region IDisposable
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            
        }
        #endregion
    }
}

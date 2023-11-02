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
        // don't want to write every single time, but rather cache for a time
        // reading are coming in every 15min, use a bigger number if want to cache readings on RPi
        const int DELAYMINUTES = 10;

        private readonly ILogger<MqttWorker> _logger;
        IMqttClient? _mqttClient;
        WorkerSettings _workerSettings;

        private static DateTime? _lastWrite;
        private DateTime LastWrite
        {
            // if no writes yet, trigger a write
            get => _lastWrite ??= DateTime.Now.AddMinutes(-(DELAYMINUTES + 1));
            set { _lastWrite = value; }
        }

        private static List<TasmotaSensor>? _readings;
        private List<TasmotaSensor> Readings
        {
            get => _readings ??= new List<TasmotaSensor>();
            set { _readings = value; }
        }

        public MqttWorker(ILogger<MqttWorker> logger)
        {
            _logger = logger;

            _workerSettings = new()
            {
                MqttServer = Environment.GetEnvironmentVariable("MQTT_SERVER"),
                MqttTopic = Environment.GetEnvironmentVariable("MQTT_TOPIC"),
                MongoDbServer = Environment.GetEnvironmentVariable("MONGODB_SERVER"),
                MongoDbCollection = Environment.GetEnvironmentVariable("MONGODB_COLLECTION")
            };

            if (!_workerSettings.Validate())
            {
                Console.WriteLine("** Environment variables missing");
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
            // add the latest reading first
            Readings.Add(data);

            if (DateTime.Now.Subtract(LastWrite).TotalMinutes > DELAYMINUTES)
            {
                _logger.LogInformation("Time to write");
                // also save the latest reading
                LastWrite = DateTime.Now;

                WriteObjects();

                // clean-up cache
                Readings.Clear();
            }
            else
            {
                _logger.LogInformation($"No write yet. LastWrite={LastWrite}");
            }
        }

        private void WriteObjects()
        {
            if (Readings.Count < 1) return;

            var client = new MongoClient(_workerSettings.MongoDbServer);

            var collection = client.GetDatabase(_workerSettings.MongoDbCollection).GetCollection<TasmotaSensor>("tasmota");

            collection.InsertMany(Readings);
        }

        #region IDisposable
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WriteObjects();
        }
        #endregion
    }
}

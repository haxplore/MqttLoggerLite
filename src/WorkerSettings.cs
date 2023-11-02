namespace MqttLoggerLite
{
    public class WorkerSettings
    {
        public string? MqttServer { get; set; }
        public string? MqttTopic { get; set; }
        public string? MongoDbServer { get; set; }
        public string? MongoDbCollection { get; set; }

        public bool Validate()
        {
            return (MqttServer != null && MqttTopic != null && MongoDbServer != null && MongoDbCollection != null);
        }
    }
}

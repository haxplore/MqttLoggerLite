namespace MqttLoggerLite
{
    public class WorkerSettings
    {
        public string? MqttServer { get; set; }
        public string? MqttTopic { get; set; }
        public string? LiteDbFile { get; set; }

        public bool Validate()
        {
            return (MqttServer != null && MqttTopic != null && LiteDbFile != null);
        }
    }
}

using System.Text.Json.Serialization;

namespace MqttLoggerLite
{
    /// <summary>
    /// For mapping objects coming from Tasmota via %TOPIC%/SENSORS
    /// </summary>
    public class TasmotaSensor
    {
        public DateTime Time { get; set; }
        [JsonPropertyName("ENERGY")]
        public TasmotaEnergy Energy { get; set; }

        public TasmotaSensor()
        {
            Energy = new TasmotaEnergy();
        }
    }

    public class TasmotaEnergy
    {
        public DateTime TotalStartTime { get; set; }
        public double Total { get; set; }
        public double Yesterday { get; set; }
        public double Today { get; set; }
        public double Period { get; set; }
        public double Power { get; set; }
        public double ApparentPower { get; set; }
        public double ReactivePower { get; set; }
        public double Factor { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
    }
}

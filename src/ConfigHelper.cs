using System.Diagnostics;
using System.Text.Json;

namespace MqttLoggerLite
{
    public static class ConfigHelper
    {
        public static WorkerSettings LoadData(string filename)
        {
            WorkerSettings result = new();
            try
            {
                var jso = new JsonSerializerOptions()
                {

                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                using (TextReader reader = new StreamReader(filename))
                {
                    var jsonConfig = reader.ReadToEnd();
                    result = JsonSerializer.Deserialize<WorkerSettings>(jsonConfig, jso);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return result;
        }
    }
}

using System.Collections.Generic;
using System.Text;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffTHSensorData : SensorData
    {
        public string Time { get; set; }
        public AM2301Data AM2301 { get; set; }
        public string TempUnit { get; set; }

        public override void Update(MqttApplicationMessage message)
        {
            UpdateValues(JsonConvert.DeserializeObject<SonoffTHSensorData>(Encoding.UTF8.GetString(message.Payload)));
        }

        public override Dictionary<string, object> ToDictionary() => new Dictionary<string, object>{
            { "Humidity", AM2301?.Humidity },
            { "Temperature", AM2301?.Temperature }
        };

        public class AM2301Data
        {
            public float Temperature { get; set; }
            public float Humidity { get; set; }
        }
    }
}

using System.Collections.Generic;
using System.Text;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffTHSensorData : SensorData
    {
        public float Temperature { get; set; }
        public float Humidity { get; set; }

        public SonoffTHSensorData() { }

        public SonoffTHSensorData(THSensorData data) {
            Temperature = data.AM2301.Temperature;
            Humidity = data.AM2301.Humidity;
        }

        public override Dictionary<string, object> Update(MqttApplicationMessage message)
        {
            return UpdateValues(new SonoffTHSensorData(JsonConvert.DeserializeObject<THSensorData>(Encoding.UTF8.GetString(message.Payload))));
        }

        public class THSensorData
        {
            public string Time { get; set; }
            public AM2301Data AM2301 { get; set; }
            public string TempUnit { get; set; }

            public class AM2301Data
            {
                public float Temperature { get; set; }
                public float Humidity { get; set; }
            }
        }
    }
}

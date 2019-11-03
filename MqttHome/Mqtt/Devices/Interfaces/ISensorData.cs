using System.Collections.Generic;
using MQTTnet;

namespace MqttHome.Mqtt
{
    public interface ISensorData
    {
        Dictionary<string, object> ToDictionary();
        void Update(MqttApplicationMessage message);
    }
}
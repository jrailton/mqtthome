using System.Collections.Generic;
using MqttHome.Devices.Serial.Axpert;
using MQTTnet;

namespace MqttHome.Mqtt
{
    public interface ISensorData
    {
        Dictionary<string, object> ToDictionary();
        Dictionary<string, object> Update(MqttApplicationMessage message);
        Dictionary<string, object> Update(byte[] data);
        Dictionary<string, object> Update(string data);
    }
}
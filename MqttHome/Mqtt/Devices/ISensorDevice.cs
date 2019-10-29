using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    public interface ISensorDevice
    {
        void ParseSensorPayload(MqttApplicationMessage e);

        bool IsSubscribedToSensorTopic(string topic);

        Dictionary<string, object> SensorValues();
    }
}

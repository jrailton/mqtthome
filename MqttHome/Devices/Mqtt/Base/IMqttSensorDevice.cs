using System;
using System.Collections.Generic;
using System.Text;
using MqttHome.Devices.Base;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    public interface IMqttSensorDevice<ISensorData> : ISensorDevice<ISensorData>
    {
        bool SaveSensorValuesToDatabase { get; }
        void ParseSensorPayload(MqttApplicationMessage e);
        List<string> SensorTopics { get; }
    }
}

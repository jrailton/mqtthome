using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    public interface ISensorDevice<TSensorData>
    {
        event EventHandler<SensorDataChangedEventArgs> SensorDataChanged;
        void ParseSensorPayload(MqttApplicationMessage e);

        bool IsSubscribedToSensorTopic(string topic);

        Dictionary<string, object> SensorValues();
        List<string> SensorTopics { get; }

        string Id { get; }

        TSensorData SensorData { get; }
    }

    public class SensorDataChangedEventArgs : EventArgs {
        public SensorData SensorData;
    }
}

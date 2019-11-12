using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    public interface ISensorDevice<ISensorData>
    {
        event EventHandler<SensorDataChangedEventArgs> SensorDataChanged;
        void ParseSensorPayload(MqttApplicationMessage e);

        DateTime? LastMqttMessage { get; set; }
        bool SaveSensorValuesToDatabase { get; }

        Dictionary<string, object> SensorValues { get; }
        List<string> SensorTopics { get; }

        string Id { get; }

        ISensorData SensorData { get; }
    }

    public class SensorDataChangedEventArgs : EventArgs {
        public Dictionary<string, object> ChangedValues;
    }
}

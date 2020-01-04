using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Devices.Base
{
    public interface ISensorDevice<ISensorData>
    {
        public event EventHandler<SensorDataChangedEventArgs> SensorDataChanged;

        DateTime? LastCommunication { get; set; }
        bool SaveSensorValuesToDatabase { get; }

        Dictionary<string, object> SensorValues { get; }

        string Id { get; }

        ISensorData SensorData { get; }
    }

    public class SensorDataChangedEventArgs : EventArgs
    {
        public Dictionary<string, object> ChangedValues;
    }
}

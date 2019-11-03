using System;
using System.Collections.Generic;
using MqttHome.Mqtt.Devices;
using MQTTnet;

namespace MqttHome.Mqtt
{
    public abstract class MqttSensorDevice<TSensorData> : MqttDevice, ISensorDevice<ISensorData> where TSensorData : SensorData, new()
    {
        public MqttSensorDevice(MqttHomeController controller, string id, MqttDeviceType type) : base(controller, id, type)
        {
            _sensorData = new TSensorData();
            DeviceClass = MqttDeviceClass.Sensor;
            SensorTopics = new List<string> {
                $"tele/{id}/SENSOR"
            };
        }

        protected ISensorData _sensorData;

        public event EventHandler<SensorDataChangedEventArgs> SensorDataChanged;

        public ISensorData SensorData
        {
            get => _sensorData;
            set
            {
                _sensorData = value;
                SensorDataChanged?.Invoke(this, new SensorDataChangedEventArgs
                {
                    SensorData = _sensorData
                });
            }
        }

        public virtual Dictionary<string, object> SensorValues => SensorData.ToDictionary();

        public virtual List<string> SensorTopics { get; set; }

        public virtual void ParseSensorPayload(MqttApplicationMessage e) {
            LastMqttMessage = DateTime.Now;
            SensorData.Update(e);
        }
    }
}

using System;
using System.Collections.Generic;
using MqttHome.Mqtt.Devices;
using MQTTnet;

namespace MqttHome.Mqtt
{
    public abstract class MqttSensorDevice<TSensorData> : MqttDevice, ISensorDevice<ISensorData> where TSensorData : SensorData, new()
    {
        public MqttSensorDevice(MqttHomeController controller, string id, string friendlyName, DeviceType type, params string[] config) : base(controller, id, friendlyName, type, config)
        {
            DeviceClass = DeviceClass.Sensor;

            SensorData = new TSensorData();
            SensorTopics = new List<string> {
                $"tele/{id}/SENSOR"
            };
        }

        public event EventHandler<SensorDataChangedEventArgs> SensorDataChanged;

        public ISensorData SensorData { get; protected set; }

        public virtual bool SaveSensorValuesToDatabase => true;

        public virtual Dictionary<string, object> SensorValues => SensorData.ToDictionary();

        public virtual List<string> SensorTopics { get; set; }

        public virtual void ParseSensorPayload(MqttApplicationMessage e) {            
            var updated = SensorData.Update(e);

            if (Controller.SaveAllSensorValuesToDatabaseEveryTime)
            {
                SensorDataChanged?.Invoke(this, new SensorDataChangedEventArgs
                {
                    ChangedValues = SensorData.ToDictionary()
                });
            }
            else if ((updated?.Count ?? 0) > 0 && SensorDataChanged != null)
            {
                SensorDataChanged?.Invoke(this, new SensorDataChangedEventArgs
                {
                    ChangedValues = updated
                });
            }
        }
    }
}

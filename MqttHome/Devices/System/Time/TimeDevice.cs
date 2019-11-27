using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MQTTnet;

namespace MqttHome.Mqtt.Devices.Environment
{
    class TimeDevice : MqttSensorDevice<TimeSensorData>
    {
        private Timer _updateValues;

        public TimeDevice(MqttHomeController controller, string id, string friendlyName) : base(controller, id, friendlyName, DeviceType.TimeSensor) {
            SensorData = new TimeSensorData(controller.Longitude, controller.Latitude);
            LastCommunication = DateTime.Now;

            // trigger a new update of values every 30 seconds
            _updateValues = new Timer((state) => {
                base.ParseSensorPayload(null);
            }, null, 5000, 30000);

        }

        public override List<string> SensorTopics { get => new List<string>(); set { } }
        public override bool SaveSensorValuesToDatabase => false;
    }
}

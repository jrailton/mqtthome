using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices.Victron
{
    public class VenusGxDevice : MqttSensorDevice<VenusGxSensorData>, ISensorDevice
    {
        public string SerialNumber;

        public VenusGxDevice(MqttHomeController controller, string id, string serialNumber) : base(controller, id)
        {
            SerialNumber = serialNumber;
        }

        public override List<string> SensorTopics => new List<string> { 
            $"N/{SerialNumber}/system/0/Dc/Battery",
            $"N/{SerialNumber}/system/0/Ac/Grid"
        };
        public override string StateTopic => null;

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.VictronCCGX;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Sensor;

        public override bool IsSubscribedToSensorTopic(string topic)
        {
            return SensorTopics.Any(t => topic.StartsWith(t));
        }

        public override void ParseStatePayload(MqttApplicationMessage message)
        {
            throw new NotImplementedException();
        }
    }
}

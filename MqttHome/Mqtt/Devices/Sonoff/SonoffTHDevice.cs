using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using InfluxDB.LineProtocol.Payload;
using MqttHome.Mqtt.Devices;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffTHDevice : MqttStatefulSensorDevice<SonoffTHSensorData>
    {
        public SonoffTHDevice(MqttHomeController controller, string id, string friendlyName) : base(controller, id, friendlyName, MqttDeviceType.SonoffTH)
        {
        }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.SonoffTH;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Switch;
    }
}

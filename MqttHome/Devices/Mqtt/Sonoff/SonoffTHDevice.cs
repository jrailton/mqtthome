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
    public class SonoffTHDevice : MqttSwitchSensorDevice<SonoffTHSensorData>
    {
        public SonoffTHDevice(MqttHomeController controller, Config.Device config) : base(controller, DeviceType.SonoffTH, config)
        {
        }

        public override DeviceType DeviceType => DeviceType.SonoffTH;
        public override DeviceClass DeviceClass => DeviceClass.Switch;
    }
}

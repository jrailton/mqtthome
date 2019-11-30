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
    public class SonoffPowR2Device : MqttSwitchSensorDevice<SonoffPowR2SensorData>
    {
        public SonoffPowR2Device(MqttHomeController controller, Config.Device config) : base(controller, DeviceType.SonoffPowR2, config)
        {
        }

        public override DeviceType DeviceType => DeviceType.SonoffPowR2;
        public override DeviceClass DeviceClass => DeviceClass.Switch;

    }
}
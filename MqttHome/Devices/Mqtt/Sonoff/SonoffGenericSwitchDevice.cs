﻿using System;
using System.Text;
using InfluxDB.LineProtocol.Payload;
using MqttHome.Mqtt.Devices;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffGenericSwitchDevice : MqttSwitchDevice
    {
        private SwitchHelper _switchHelper;
        public SonoffGenericSwitchDevice(MqttHomeController controller, DeviceType type, Config.Device config) : base(controller, type, config)
        {
            _switchHelper = new SwitchHelper(this);
        }

        public override DeviceType DeviceType => DeviceType.Unknown;
        public override DeviceClass DeviceClass => DeviceClass.Switch;

        public override void ParseStatePayload(MqttApplicationMessage message)
        {
            var state = JsonConvert.DeserializeObject<SonoffGenericStateData>(Encoding.UTF8.GetString(message.Payload));

            var newState = state.POWER.Equals("ON", StringComparison.CurrentCultureIgnoreCase);
            if (PowerOn != newState)
                PowerOn = newState;
        }
    }
}

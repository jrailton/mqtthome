﻿using System.Text;
using InfluxDB.LineProtocol.Payload;
using MQTTnet;
using Newtonsoft.Json;

namespace InfluxDbLoader.Mqtt
{
    public class SonoffPowR2Device : MqttDevice
    {
        public SonoffPowR2Device(string id)
            : base(id)
        {
            SetPowerStateOn = new MqttCommand(id, $"cmnd/{id}/Power", "ON");
            SetPowerStateOff = new MqttCommand(id, $"cmnd/{id}/Power", "OFF");
        }

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.SonoffPowR2;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Switch;

        public override void ParseStatePayload(MqttApplicationMessage message)
        {
            PowerOn = JsonConvert.DeserializeObject<SonoffGenericStateData>(Encoding.UTF8.GetString(message.Payload)).PowerOn;
        }

        public override void ParseSensorPayload(MqttApplicationMessage message)
        {
            SensorData = JsonConvert.DeserializeObject<SonoffPowR2SensorData>(Encoding.UTF8.GetString(message.Payload));
        }
    }
}
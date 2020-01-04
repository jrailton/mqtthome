using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;
using log4net;
using MqttHome.Influx;
using MqttHome.Mqtt.Devices;

namespace MqttHome.Devices.Serial.Base
{
    public abstract class SerialDevice : Device, ISerialDevice, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id is the device ID that will be used as the topic and influx data tag</param>
        public SerialDevice(MqttHomeController controller, DeviceType type, Config.Device config) : base(controller, config)
        {
            DeviceType = type;
            Controller.DeviceLog.Debug($"Adding {DeviceType} {DeviceClass} device {Id}");
        }

        public DateTime? LastCommunication { get; set; }

        public abstract void Dispose();
    }
}

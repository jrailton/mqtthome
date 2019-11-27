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
    public abstract class SerialDevice : Device, ISerialDevice
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id is the device ID that will be used as the topic and influx data tag</param>
        public SerialDevice(MqttHomeController controller, string id, string friendlyName, DeviceType type, params string[] config) : base(controller, id, friendlyName)
        {
            DeviceType = type;
            Controller.DeviceLog.Debug($"Adding {DeviceType} {DeviceClass} device {id}");
        }

        public DateTime? LastCommunication { get; set; }
    }
}

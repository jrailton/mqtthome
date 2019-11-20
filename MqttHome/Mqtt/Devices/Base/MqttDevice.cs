using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;
using log4net;
using MqttHome.Influx;
using MqttHome.Mqtt.Devices;

namespace MqttHome.Mqtt
{
    public abstract class MqttDevice : Device, IMqttDevice
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id is the device ID that will be used as the topic and influx data tag</param>
        public MqttDevice(MqttHomeController controller, string id, string friendlyName, DeviceType type, params string[] config) : base(controller, id, friendlyName)
        {
            DeviceType = type;
            Controller.DeviceLog.Debug($"Adding {DeviceType} {DeviceClass} device {id}");
        }

        public DateTime? LastMqttMessage { get; set; }

        public virtual List<string> AllTopics
        {
            get
            {
                try
                {
                    var topics = new List<string>();

                    if (this is IStatefulDevice)
                    {
                        var sd = this as IStatefulDevice;
                        topics.Add(sd.CommandResponseTopic);
                        topics.Add(sd.StateTopic);
                    }

                    if (this is ISensorDevice<ISensorData>)
                    {
                        var sd = this as ISensorDevice<ISensorData>;
                        topics.AddRange(sd.SensorTopics);
                    }

                    return topics.Where(s => !string.IsNullOrEmpty(s)).ToList();
                }
                catch
                {
                    throw;
                }
            }
        }

        // commands
        public virtual MqttCommand RebootCommand
        {
            get { return new MqttCommand(Controller, Id, $"cmnd/{Id}/Restart", "1"); }
            set { }
        }

    }
}

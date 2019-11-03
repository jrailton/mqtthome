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
    public abstract class MqttDevice
    {
        public MqttHomeController Controller { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id is the device ID that will be used as the topic and influx data tag</param>
        public MqttDevice(MqttHomeController controller, string id, MqttDeviceType type)
        {
            DeviceType = type;
            Controller = controller;
            Id = id;
            Controller.DeviceLog.Debug($"Adding {DeviceType} {DeviceClass} device {id}");
        }

        public string Id { get; set; }

        public DateTime? LastMqttMessage { get; set; }

        public virtual List<string> AllTopics {
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
                catch (Exception err) {
                    throw;
                }
            }
        }

        // commands
        public virtual MqttCommand RebootDevice
        {
            get { return new MqttCommand(Controller, Id, $"cmnd/{Id}/Restart", "1"); }
            set { }
        }

        public abstract MqttDeviceType DeviceType { get; set; }
        public abstract MqttDeviceClass DeviceClass { get; set; }

    }
}

﻿using System;
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
        public MqttDevice(MqttHomeController controller, DeviceType type, Config.Device config) : base(controller, config)
        {
            DeviceType = type;
        }

        public DateTime? LastCommunication { get; set; }

        public virtual List<string> AllTopics
        {
            get
            {
                try
                {
                    var topics = new List<string>();

                    if (this is ISwitchDevice)
                    {
                        var sd = this as ISwitchDevice;
                        topics.Add(sd.CommandResponseTopic);
                        topics.Add(sd.StateTopic);
                    }

                    if (this is IMqttSensorDevice<ISensorData>)
                    {
                        var sd = this as IMqttSensorDevice<ISensorData>;
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

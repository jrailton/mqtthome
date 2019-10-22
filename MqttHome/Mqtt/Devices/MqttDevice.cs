using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;
using MQTTnet;

namespace InfluxDbLoader.Mqtt
{
    public abstract class MqttDevice
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id is the device ID that will be used as the topic and influx data tag</param>
        public MqttDevice(string id)
        {
            Id = id;
        }

        public string Id;
        public virtual string StateTopic
        {
            get => $"tele/{Id}/STATE";
            set { }
        }

        public virtual string CommandResponseTopic
        {
            get => $"stat/{Id}/POWER";
            set { }
        }

        // commands
        public virtual MqttCommand SetPowerStateOn { get; set; }
        public virtual MqttCommand SetPowerStateOff { get; set; }
        public virtual MqttCommand RebootDevice
        {
            get { return new MqttCommand(Id, $"cmnd/{Id}/Restart", "1"); }
            set { }
        }

        public virtual List<string> SensorTopics
        {
            get => new List<string>{ $"tele/{Id}/SENSOR" };
            set { }
        }
        public abstract MqttDeviceType DeviceType { get; set; }
        public abstract MqttDeviceClass DeviceClass { get; set; }

        private bool _powerOn;
        public bool PowerOn
        {
            get => _powerOn;
            protected set
            {
                _powerOn = value;
                WriteToInflux(_powerOn);
            }
        }

        private SensorData _sensorData;
        public SensorData SensorData
        {
            get => _sensorData;
            set
            {
                _sensorData = value;
                WriteToInflux(_sensorData);
            }
        }

        public bool IsSubscribedToStateTopic(string topic)
        {
            return (StateTopic ?? "").Equals(topic, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool IsSubscribedToCommandResponseTopic(string topic)
        {
            return (CommandResponseTopic ?? "").Equals(topic, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool IsSubscribedToSensorTopic(string topic)
        {
            return (SensorTopics ?? new List<string>()).Contains(topic, StringComparer.CurrentCultureIgnoreCase);
        }

        public abstract void ParseStatePayload(MqttApplicationMessage message);
        public abstract void ParseSensorPayload(MqttApplicationMessage message);

        /// <summary>
        /// This works for Sonoff Tasmota devices -- will need to be overridden for others
        /// </summary>
        public virtual void ParseCommandResponsePayload(MqttApplicationMessage message)
        {
            var test = new Regex(@"^(?<topic>.+)\/(?<device>.+)\/(?<subject>.+)$");
            var match = test.Match(message.Topic);
            var payload = Encoding.UTF8.GetString(message.Payload);

            if (match.Success)
            {
                switch (match.Groups["subject"].Value)
                {
                    case "POWER":
                        PowerOn = payload.Equals("ON", StringComparison.CurrentCultureIgnoreCase);
                        break;

                    default:
                        break;
                }
            }
        }

        private void WriteToInflux(SensorData data)
        {
            var lpp = new LineProtocolPoint(DeviceClass.ToString(),
                data.ToDictionary(),
                new Dictionary<string, string>
                {
                    {"device", Id}
                });

            Program.InfluxCommunicator.Write(lpp);
        }

        private void WriteToInflux(bool powerOn)
        {
            var lpp = new LineProtocolPoint("Switch",
                new Dictionary<string, object>{
                    { "On", (powerOn ? "1" : "0") }
                },
                new Dictionary<string, string>
                {
                    {"device", Id}
                });

            Program.InfluxCommunicator.Write(lpp);
        }

        public void SwitchOn()
        {
            SetPowerStateOn.Execute();
        }

        public void SwitchOff()
        {
            SetPowerStateOff.Execute();
        }
    }

    public class MqttCommand
    {
        public MqttCommand(string deviceId, string topic, string payload) : this(deviceId, topic, Encoding.UTF8.GetBytes(payload))
        {
        }

        public MqttCommand(string deviceId, string topic, IEnumerable<byte> payload)
        {
            Topic = topic;
            Payload = payload;
            DeviceId = deviceId;
        }

        public string DeviceId { get; set; }
        public string Topic { get; set; }
        public IEnumerable<byte> Payload { get; set; }

        public void Execute()
        {
            Program.MqttCommunicator.PublishCommand(this);
        }
    }
}

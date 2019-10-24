using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;
using log4net;
using MqttHome.Influx;
using MqttHome.Mqtt.Devices;
using MQTTnet;

namespace MqttHome.Mqtt
{
    public abstract class MqttDevice
    {
        private MqttHomeController _controller;

        public delegate void StateChange(MqttDeviceState state);

        public event StateChange StateChangeEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id is the device ID that will be used as the topic and influx data tag</param>
        public MqttDevice(MqttHomeController controller, string id)
        {
            _controller = controller;
            Id = id;
            _controller.DeviceLog.Debug($"Adding {DeviceType} {DeviceClass} device {id}");
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

        public IEnumerable<string> AllTopics {
            get
            {
                var topics = SensorTopics;
                topics.Add(StateTopic);
                topics.Add(CommandResponseTopic);
                return topics.Where(s => !string.IsNullOrEmpty(s));
            }
        }

        // commands
        protected virtual MqttCommand SetPowerStateOn { get; set; }
        protected virtual MqttCommand SetPowerStateOff { get; set; }
        public virtual MqttCommand RebootDevice
        {
            get { return new MqttCommand(_controller, Id, $"cmnd/{Id}/Restart", "1"); }
            set { }
        }

        public virtual List<string> SensorTopics
        {
            get => new List<string>{ $"tele/{Id}/SENSOR" };
            set { }
        }
        public abstract MqttDeviceType DeviceType { get; set; }
        public abstract MqttDeviceClass DeviceClass { get; set; }
        public DateTime? PowerOffTime { get; private set; }

        private bool _powerOn;
        public bool PowerOn
        {
            get => _powerOn;
            protected set
            {
                _powerOn = value;

                if (_powerOn)
                {
                    // clear power off time (used for flipflop prevention)
                    PowerOffTime = null;
                }
                else {
                    // maintain power off time if its already set
                    PowerOffTime = PowerOffTime ?? DateTime.Now;
                }

                StateChangeEvent?.Invoke(new MqttDeviceState{
                    Device = this,
                    PowerOn = value,
                    Type = eMqttDeviceStateChangeType.Power,
                    SensorData = _sensorData
                });
            }
        }

        private SensorData _sensorData;
        public SensorData SensorData
        {
            get => _sensorData;
            set
            {
                _sensorData = value;
                StateChangeEvent?.Invoke(new MqttDeviceState
                {
                    Device = this,
                    PowerOn = _powerOn,
                    Type = eMqttDeviceStateChangeType.SensorData,
                    SensorData = _sensorData
                });
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

        public void SwitchOn(string reason, int? flipFlopSeconds)
        {
            // default to 15 seconds if null
            flipFlopSeconds = flipFlopSeconds ?? 15;

            _controller.DeviceLog.Info($"SwitchOn :: Reason - {reason}");

            // prevent flipflop
            if (PowerOffTime.HasValue && PowerOffTime.Value.AddSeconds(flipFlopSeconds.Value) > DateTime.Now)
            {
                var error = $"Flipflop prevention. Need to wait until {PowerOffTime.Value.AddSeconds(flipFlopSeconds.Value).ToString("HH:mm:ss")}";
                _controller.DeviceLog.Warn($"SwitchOn :: Reason - {reason} :: Aborted - {error}");
                throw new Exception(error);
            }
            else
            {
                SetPowerStateOn.Execute();
            }
        }

        public void SwitchOff(string reason)
        {
            _controller.DeviceLog.Info($"SwitchOff :: Reason - {reason}");
            SetPowerStateOff.Execute();
        }
    }
}

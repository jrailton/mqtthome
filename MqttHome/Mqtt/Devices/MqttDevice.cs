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
using Newtonsoft.Json;

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

        public virtual List<string> AllTopics {
            get
            {
                var topics = new List<string>();

                if (this is IStatefulDevice)
                {
                    var sd = this as IStatefulDevice;
                    topics.Add(sd.CommandResponseTopic);
                    topics.Add(sd.StateTopic);
                }

                if (this is ISensorDevice<SensorData>)
                {
                    var sd = this as ISensorDevice<SensorData>;
                    topics.AddRange(sd.SensorTopics);
                }

                return topics.Where(s => !string.IsNullOrEmpty(s)).ToList();
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

    public abstract class MqttStatefulDevice : MqttDevice, IStatefulDevice
    {
        private SwitchHelper _switchHelper;

        public MqttStatefulDevice(MqttHomeController controller, string id, MqttDeviceType type) : base(controller, id, type)
        {
            DeviceClass = MqttDeviceClass.Switch;
            SetPowerStateOn = new MqttCommand(controller, id, $"cmnd/{id}/Power", "ON");
            SetPowerStateOff = new MqttCommand(controller, id, $"cmnd/{id}/Power", "OFF");
            _switchHelper = new SwitchHelper(this);
        }


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


        public bool IsSubscribedToStateTopic(string topic)
        {
            return (StateTopic ?? "").Equals(topic, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool IsSubscribedToCommandResponseTopic(string topic)
        {
            return (CommandResponseTopic ?? "").Equals(topic, StringComparison.CurrentCultureIgnoreCase);
        }

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

        public DateTime? PowerOffTime { get; private set; }

        protected bool _powerOn;

        public event EventHandler<StateChangedEventArgs> StateChanged;

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
                else
                {
                    // maintain power off time if its already set
                    PowerOffTime = PowerOffTime ?? DateTime.Now;
                }

                StateChanged?.Invoke(this, new StateChangedEventArgs
                {
                    PowerOn = value,
                });
            }
        }

        public MqttCommand SetPowerStateOn { get; private set; }

        public MqttCommand SetPowerStateOff { get; private set; }

        public abstract void ParseStatePayload(MqttApplicationMessage message);

        public void SwitchOff(string reason)
        {
            _switchHelper.SwitchOff(reason);
        }

        public void SwitchOn(string reason, int? flipFlopSeconds)
        {
            _switchHelper.SwitchOn(reason, flipFlopSeconds);
        }
    }

    public abstract class MqttSensorDevice<TSensorData> : MqttDevice, ISensorDevice<TSensorData> where TSensorData : SensorData, new()
    {
        public MqttSensorDevice(MqttHomeController controller, string id, MqttDeviceType type) : base(controller, id, type)
        {
            _sensorData = new TSensorData();
            DeviceClass = MqttDeviceClass.Sensor;
        }

        protected TSensorData _sensorData;

        public event EventHandler<SensorDataChangedEventArgs> SensorDataChanged;

        public TSensorData SensorData
        {
            get => _sensorData;
            set
            {
                _sensorData = value;
                SensorDataChanged(this, new SensorDataChangedEventArgs
                {
                    SensorData = _sensorData
                });
            }
        }

        public virtual Dictionary<string, object> SensorValues()
        {
            return SensorData.ToDictionary();
        }

        public virtual List<string> SensorTopics
        {
            get => new List<string> { $"tele/{Id}/SENSOR" };
            set { }
        }

        public virtual bool IsSubscribedToSensorTopic(string topic)
        {
            return (SensorTopics ?? new List<string>()).Contains(topic, StringComparer.CurrentCultureIgnoreCase);
        }

        public virtual void ParseSensorPayload(MqttApplicationMessage e) {
            SensorData.Update(e);
        }
    }

    public abstract class MqttStatefulSensorDevice<TSensorData> : MqttSensorDevice<TSensorData>, ISensorDevice<TSensorData>, IStatefulDevice where TSensorData : SensorData, new()
    {

        private SwitchHelper _switchHelper;

        public MqttStatefulSensorDevice(MqttHomeController controller, string id, MqttDeviceType type) : base(controller, id, type)
        {
            DeviceClass = MqttDeviceClass.Combo;
            SetPowerStateOn = new MqttCommand(controller, id, $"cmnd/{id}/Power", "ON");
            SetPowerStateOff = new MqttCommand(controller, id, $"cmnd/{id}/Power", "OFF");
            _switchHelper = new SwitchHelper(this);
        }


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


        public bool IsSubscribedToStateTopic(string topic)
        {
            return (StateTopic ?? "").Equals(topic, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool IsSubscribedToCommandResponseTopic(string topic)
        {
            return (CommandResponseTopic ?? "").Equals(topic, StringComparison.CurrentCultureIgnoreCase);
        }

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

        public DateTime? PowerOffTime { get; private set; }

        protected bool _powerOn;

        public event EventHandler<StateChangedEventArgs> StateChanged;

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
                else
                {
                    // maintain power off time if its already set
                    PowerOffTime = PowerOffTime ?? DateTime.Now;
                }

                StateChanged?.Invoke(this, new StateChangedEventArgs
                {
                    PowerOn = value
                });
            }
        }

        public MqttCommand SetPowerStateOn { get; private set; }

        public MqttCommand SetPowerStateOff { get; private set; }

        public virtual void ParseStatePayload(MqttApplicationMessage message)
        {
            var state = JsonConvert.DeserializeObject<SonoffGenericStateData>(Encoding.UTF8.GetString(message.Payload));
            PowerOn = state.POWER.Equals("ON", StringComparison.CurrentCultureIgnoreCase);
        }

        public void SwitchOff(string reason)
        {
            _switchHelper.SwitchOff(reason);
        }

        public void SwitchOn(string reason, int? flipFlopSeconds)
        {
            _switchHelper.SwitchOn(reason, flipFlopSeconds);
        }
    }
}

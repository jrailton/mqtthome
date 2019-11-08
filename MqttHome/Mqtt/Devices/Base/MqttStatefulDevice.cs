using System;
using System.Text;
using System.Text.RegularExpressions;
using MqttHome.Mqtt.Devices;
using MQTTnet;

namespace MqttHome.Mqtt
{
    public abstract class MqttStatefulDevice : MqttDevice, IStatefulDevice
    {
        public event EventHandler<StateChangedEventArgs> StateChanged;

        private SwitchHelper _switchHelper;

        public MqttStatefulDevice(MqttHomeController controller, string id, string friendlyName, MqttDeviceType type, params string[] config) : base(controller, id, friendlyName, type, config)
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

        /// <summary>
        /// This works for Sonoff Tasmota devices -- will need to be overridden for others
        /// </summary>
        public virtual void ParseCommandResponsePayload(MqttApplicationMessage message)
        {
            LastMqttMessage = DateTime.Now;

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

        protected bool? _powerOn;

        public bool? PowerOn
        {
            get => _powerOn;
            protected set
            {
                _powerOn = value;

                if (_powerOn.Value)
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
                    PowerOn = value.Value,
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
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MqttHome.Mqtt.Devices;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public abstract class MqttSwitchSensorDevice<TSensorData> : MqttSensorDevice<TSensorData>, IMqttSensorDevice<ISensorData>, ISwitchDevice where TSensorData : SensorData, new()
    {
        private SwitchHelper _switchHelper;

        public MqttSwitchSensorDevice(MqttHomeController controller, DeviceType type, Config.Device config) : base(controller, type, config)
        {
            SetPowerStateOn = new MqttCommand(controller, Id, $"cmnd/{Id}/Power", "ON");
            SetPowerStateOff = new MqttCommand(controller, Id, $"cmnd/{Id}/Power", "OFF");
            StateTopic = $"tele/{Id}/STATE";
            CommandResponseTopic = $"stat/{Id}/POWER";

            _switchHelper = new SwitchHelper(this);
        }


        public virtual string StateTopic { get; set; }

        public virtual string CommandResponseTopic { get; set; }

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

        protected bool? _powerOn;

        public event EventHandler<StateChangedEventArgs> StateChanged;

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

                _switchHelper.AddStateHistory($"State changed to {(value.Value ? "ON" : "OFF")}");

                StateChanged?.Invoke(this, new StateChangedEventArgs
                {
                    PowerOn = value.Value
                });
            }
        }

        public MqttCommand SetPowerStateOn { get; private set; }

        public MqttCommand SetPowerStateOff { get; private set; }

        public Dictionary<DateTime, string> StateHistory => _switchHelper.StateHistory;

        public string StateQuery => _switchHelper.StateQuery;

        public virtual void ParseStatePayload(MqttApplicationMessage message)
        {
            var state = JsonConvert.DeserializeObject<SonoffGenericStateData>(Encoding.UTF8.GetString(message.Payload));
            
            var newState = state.POWER.Equals("ON", StringComparison.CurrentCultureIgnoreCase);
            if (PowerOn != newState)
                PowerOn = newState;
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

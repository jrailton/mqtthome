﻿using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    public interface ISwitchDevice
    {
        DateTime? LastCommunication { get; set; }

        MqttHomeController Controller { get; }

        event EventHandler<StateChangedEventArgs> StateChanged;
        void ParseStatePayload(MqttApplicationMessage message);

        string StateTopic { get; set; }

        string CommandResponseTopic { get; set; }

        void ParseCommandResponsePayload(MqttApplicationMessage message);

        DateTime? PowerOffTime { get; }

        bool? PowerOn { get; }

        string Id { get; }

        void SwitchOn(string reason, int? flipFlopSeconds);

        void SwitchOff(string reason);

        MqttCommand SetPowerStateOn { get; }
        MqttCommand SetPowerStateOff { get; }

        public Dictionary<DateTime, string> StateHistory { get; }

        public string StateQuery { get; }
    }
    public class StateChangedEventArgs : EventArgs
    {
        public bool PowerOn;
    }
}

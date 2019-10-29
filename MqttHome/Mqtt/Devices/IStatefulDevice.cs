﻿using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    public interface IStatefulDevice
    {
        void ParseStatePayload(MqttApplicationMessage message);
    }
}

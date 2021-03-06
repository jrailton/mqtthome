﻿using MqttHome.Devices.Base;
using MqttHome.Mqtt;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Devices.Serial.Base
{
    public abstract class SerialSensorDevice<TSensorData> : SerialDevice, ISensorDevice<ISensorData> where TSensorData : SensorData, new()
    {
        public SerialSensorDevice(MqttHomeController controller, DeviceType type, Config.Device config) : base(controller, type, config)
        {
            DeviceClass = DeviceClass.Sensor;
            SensorData = new TSensorData();
        }

        public event EventHandler<SensorDataChangedEventArgs> SensorDataChanged;

        public ISensorData SensorData { get; protected set; }

        public virtual bool SaveSensorValuesToDatabase => true;

        public virtual Dictionary<string, object> SensorValues => SensorData.ToDictionary();
    }
}

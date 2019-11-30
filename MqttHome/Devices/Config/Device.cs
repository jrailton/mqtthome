using System;
using System.Collections.Generic;

namespace MqttHome.Config
{
    public class Device
    {
        /// <summary>
        /// The device Id to refer to it as, usually the same as the MQTT 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// MqttDevice type (class) name
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Reserved for future use, whereby the constructor of the device requires more than the default parameters
        /// </summary>
        public string[] Parameters { get; set; }
        /// <summary>
        /// Friendly name for the device
        /// </summary>
        public string FriendlyName { get; set; }
        /// <summary>
        /// Applies to sensor devices. Whether or not the sensor values should be saved to database (for use by Grafana)
        /// </summary>
        public bool SaveSensorValuesToDatabase { get; set; } = true;
    }
}

using System;

namespace MqttHome.Mqtt
{
    /// <summary>
    /// tele/<id>/STATE
    /// </summary>
    public class SonoffGenericStateData : SwitchState
    {
        public DateTime Time { get; set; }
        public string Uptime { get; set; }
        public int Heap { get; set; }
        public string SleepMode { get; set; }
        public int Sleep { get; set; }
        public int LoadAvg { get; set; }
        public string POWER { get; set; }
        public SonoffWifiStatusData Wifi { get; set; }

        public override bool PowerOn => POWER.Equals("ON", StringComparison.CurrentCultureIgnoreCase);
    }
}

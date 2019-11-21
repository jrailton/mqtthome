using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{
    public class ICCSensorData : SensorData
    {
        public ICCSensorData()
        {
            // set default sensor state here -- could just remain blank
        }

        public ICCSensorData(MqttApplicationMessage mqtt)
        {
            var message = Encoding.UTF8.GetString(mqtt.Payload);
            string[] array;

            switch (mqtt.Topic)
            {
                case "Inverter/AllValues":
                    // 0 loadwatts, 1 grid voltage, 2 pvwatts, 3 loadwatts, 4 loadpcnt, 5 invertertemp, 6 grid watts??, 7 batteryvolts, 8 batterysoc, 9 batteryamps, 10 inverterfreq, 11 grid freq, 12 batterywatts, 13 x, 14 x

                    array = message.Split(' ');
                    LoadWatts = decimal.Parse(array[0]);
                    GridVoltage = decimal.Parse(array[1]);
                    PvWatts = decimal.Parse(array[2]);
                    LoadWatts = decimal.Parse(array[3]);
                    LoadPcnt = decimal.Parse(array[4]);
                    InverterTemp = decimal.Parse(array[5]);
                    GridWatts = decimal.Parse(array[6]); // not sure ??
                    BatteryVoltage = decimal.Parse(array[7]);
                    BatterySoC = decimal.Parse(array[8]);
                    BatteryAmps = decimal.Parse(array[9]);
                    InverterFreq = decimal.Parse(array[10]);
                    GridFreq = decimal.Parse(array[11]);
                    BatteryWatts = decimal.Parse(array[12]);

                    break;

                case "Inverter/AllValues2":
                    // 0.00 87.00                   0.00 0.00 236.60        0.00 1    Axpert5kvaSingle 316:04:19     230.70            B
                    // 0 x, 1 inverter battery soc, 2 x, 3 x, 4 pv voltage, 5 x, 6 x, 7 inverter name, 8 icc uptime, 9 inverter volts, 10 inverter mode

                    array = message.Split(' ');
                    PvVolts = decimal.Parse(array[4]);
                    IccUptimeSeconds = DeriveSeconds(array[8]);
                    InverterVolts = decimal.Parse(array[9]);
                    InverterMode = array[10];

                    break;

                case "Pylontech/Cycles":
                    PylontechCycles = int.Parse(message);
                    break;

                case "Pylontech/Watts":
                    PylontechWatts = decimal.Parse(message);
                    break;

                case "Pylontech/Temperature":
                    PylontechTemp = decimal.Parse(message);
                    break;

                case "Pylontech/Remaining_AH":
                    PylontechAhRemaining = decimal.Parse(message);
                    break;

                case "Pylontech/TimeRemaining":
                    PylontechSecondsRemaining = DeriveSeconds(message);
                    break;

                case "Pylontech/AH_Use":
                    PylontechAhUsed = decimal.Parse(message);
                    break;

                case "Pylontech/AH_Remaining_Till_20SOC":
                    PylontechAhRemainingTill20Soc = decimal.Parse(message);
                    break;
            }
        }

        public override Dictionary<string, object> Update(MqttApplicationMessage message)
        {
            return UpdateValues(new ICCSensorData(message));
        }

        /// <summary>
        /// ICC timespan is reported as hours:minutes:seconds which isnt nice for logging so convert it to seconds (integer)
        /// </summary>
        private int DeriveSeconds(string timespan)
        {
            try
            {
                if (timespan == " --- ")
                    return 0;

                int output = 0;
                var array = timespan.Split(':');
                var multipliers = new[] { 3600, 60, 1 };

                for (int i = array.Length - 1; i >= 0; i--)
                    output += (int.Parse(array[i]) * multipliers[i]);

                return output;
            }
            catch (Exception err) {
                return 0;
            }
        }

        public int? PylontechCycles { get; set; }
        public decimal? PylontechWatts { get; set; }
        public decimal? PylontechTemp { get; set; }
        public decimal? PylontechAhRemaining { get; set; }
        public decimal? PylontechAhRemainingTill20Soc { get; set; }
        public int? PylontechSecondsRemaining { get; set; }
        public decimal? PylontechAhUsed { get; set; }

        public decimal? LoadWatts { get; set; }
        public decimal? BatteryAmps { get; set; }
        public decimal? PvWatts { get; set; }
        public decimal? PvVolts { get; set; }
        public decimal? LoadPcnt { get; set; }
        public decimal? InverterTemp { get; set; }
        public decimal? InverterFreq { get; set; }
        public decimal? InverterVolts { get; set; }
        public string InverterMode { get; set; }
        public decimal? BatteryVoltage { get; set; }
        public decimal? BatterySoC { get; set; }
        public decimal? BatteryWatts { get; set; }
        public decimal? GridVoltage { get; set; }
        public decimal? GridWatts { get; set; }
        public decimal? GridFreq { get; set; }
        public int? IccUptimeSeconds { get; set; }
    }
}

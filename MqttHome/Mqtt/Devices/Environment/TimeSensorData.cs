using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MQTTnet;

namespace MqttHome.Mqtt.Devices.Environment
{
    class TimeSensorData : SensorData
    {
        public override Dictionary<string, object> Update(MqttApplicationMessage message)
        {
            return new Dictionary<string, object>();
        }

        public int Hour => DateTime.Now.Hour;
        public int Minute => DateTime.Now.Minute;
        public int Second => DateTime.Now.Second;
        public int DayOfYear => DateTime.Now.DayOfYear;
        public int DayOfWeek => (int)DateTime.Now.DayOfWeek;
        public int DayOfMonth => DateTime.Now.Day;
        public int Year => DateTime.Now.Year;
        public int Month => DateTime.Now.Month;
        public int WeekOfYear => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday);

    }
}

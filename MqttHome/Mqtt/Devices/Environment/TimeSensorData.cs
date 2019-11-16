using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using MqttHome.Devices.Helpers;
using MQTTnet;

namespace MqttHome.Mqtt.Devices.Environment
{
    class TimeSensorData : SensorData
    {
        private Timer _updateSunriseSunset;

        public TimeSensorData() { }

        public TimeSensorData(double longitude, double latitude) {
            _updateSunriseSunset = new Timer((state) =>
            {
                var today = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeKind.Utc);
                var jd = SunriseSunset.calcJD(DateTime.Now);
                Sunset = today.AddMinutes(SunriseSunset.calcSunSetUTC(jd, latitude, longitude)).ToLocalTime();
                Sunrise = today.AddMinutes(SunriseSunset.calcSunRiseUTC(jd, latitude, longitude)).ToLocalTime();
            }, null, 0, 1000 * 60 * 60 * 24);

        }

        public override Dictionary<string, object> Update(MqttApplicationMessage message)
        {
            return this.ToDictionary();
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
        public bool Sundown => DateTime.Now >= Sunset && DateTime.Now <= Sunrise.AddDays(1); // after todays sunset and before tomorrow's sunrise
        public DateTime Sunset { get; set; }
        public DateTime Sunrise { get; set; }
    }
}

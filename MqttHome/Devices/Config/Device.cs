using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;

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

        public List<WidgetConfig> Widgets { get; set; }
    }

    public class WidgetConfig
    {
        public string Name { get; set; }
        public string ValueName { get; set; }

        public WidgetValueType ValueType { get; set; }

        public WidgetType Type { get; set; } = WidgetType.Text;

        public GaugeConfig Gauge { get; set; }

        public IHtmlContent FormattedValue(object value)
        {
            try
            {
                switch (ValueType)
                {
                    case WidgetValueType.Number:
                        return new HtmlString(decimal.Parse(value.ToString()).ToString("#,##0.##"));
                    case WidgetValueType.Percent:
                        return new HtmlString($"{decimal.Parse(value.ToString()).ToString("#,##0.##")}%");
                    case WidgetValueType.Temperature:
                        return new HtmlString($"{decimal.Parse(value.ToString()).ToString("#,##0.##")}°C");
                    case WidgetValueType.Watts:
                        return new HtmlString(FormattedValueConvert(value.ToString(), new List<decimal> { 0.001m, 1, 1000, 1000000 }, new List<string> { "mW", "W", "kW", "MW" }));
                }
            }
            catch
            {
            }

            return new HtmlString(value.ToString());
        }

        private string FormattedValueConvert(string value, List<decimal> divisors, List<string> labels) {
            var v = decimal.Parse(value);

            /* e.g. value = 1270, divisors = 0.001m, 1, 1000, 1000000, labels = "mW", "W", "kW", "MW"
             * 1.27kW
             * e.g. value = 0.1270, divisors = 0.001m, 1, 1000, 1000000, labels = "mW", "W", "kW", "MW"
             * 127mW
             */

            if (divisors.Count > 1) {
                for (var i = divisors.Count - 1; i > 0; i--) {
                    if (Math.Abs(v) > divisors[i])
                        return $"{(v / divisors[i]).ToString("#,##0.##")}{labels[i]}";
                }
            }

            return $"{v.ToString("#,##0.##")}{labels[0]}";
        }
    }

    public class GaugeConfig
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 100;
        public List<string> Colours { get; set; } = new List<string> { "Green", "Orange", "Red" };
        public List<int> ColourValues { get; set; } = new List<int> { 30, 70 };
    }

    public enum WidgetType
    {
        Text = 0,
        Gauge = 1
    }

    public enum WidgetValueType
    {
        Text = 0,
        Number = 1,
        Watts = 2,
        Percent = 3,
        Temperature = 4
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt.Devices
{
    public class VenusGxSensorData : SensorData
    {
        // BATTERY
        //N/7c386655e76b/system/0/Dc/Battery/Alarms/CircuitBreakerTripped {"value": null}
        //N/7c386655e76b/system/0/Dc/Battery/ConsumedAmphours {"value": null}
        //N/7c386655e76b/system/0/Dc/Battery/Current {"value": 0.0}
        //N/7c386655e76b/system/0/Dc/Battery/Current {"value": 0.0}
        //N/7c386655e76b/system/0/Dc/Battery/Current {"value": 0.30000001192092896}
        //N/7c386655e76b/system/0/Dc/Battery/Power {"value": 0}
        //N/7c386655e76b/system/0/Dc/Battery/Power {"value": 0}
        //N/7c386655e76b/system/0/Dc/Battery/Power {"value": 15}
        //N/7c386655e76b/system/0/Dc/Battery/Soc {"value": 100}
        //N/7c386655e76b/system/0/Dc/Battery/State {"value": 0}
        //N/7c386655e76b/system/0/Dc/Battery/Temperature {"value": 25.600000381469727}
        //N/7c386655e76b/system/0/Dc/Battery/TemperatureService {"value": "com.victronenergy.battery.socketcan_can1"}
        //N/7c386655e76b/system/0/Dc/Battery/TimeToGo {"value": null}
        //N/7c386655e76b/system/0/Dc/Battery/Voltage {"value": 52.979999542236328}

        public VenusGxSensorData()
        {
            // set default sensor state here -- could just remain blank
        }

        public VenusGxSensorData(MqttApplicationMessage mqtt)
        {
            var message = Encoding.UTF8.GetString(mqtt.Payload);

            var topicArray = mqtt.Topic.Split('/');

            switch (topicArray[5])
            {
                case "Battery":
                    // 0 loadwatts, 1 grid voltage, 2 pvwatts, 3 loadwatts, 4 loadpcnt, 5 invertertemp, 6 grid watts??, 7 batteryvolts, 8 batterysoc, 9 batteryamps, 10 inverterfreq, 11 grid freq, 12 batterywatts, 13 x, 14 x

                    UpdateBatteryValues(topicArray, message);

                    break;
            }
        }

        private void UpdateBatteryValues(string[] topicArray, string message)
        {
            var value = JsonConvert.DeserializeObject<VictronMqttPayload>(message).value;

            switch (topicArray[6])
            {
                case "ConsumedAmphours":
                    BatteryConsumedAmphours = decimal.Parse(value);
                    break;

                case "Current":
                    BatteryCurrent = decimal.Parse(value);
                    break;

                case "Power":
                    BatteryPower = decimal.Parse(value);
                    break;

                case "Soc":
                    BatterySoC = decimal.Parse(value);
                    break;

                case "State":
                    BatteryState = int.Parse(value);
                    break;

                case "Temperature":
                    BatteryTemperature = decimal.Parse(value);
                    break;

                case "TimeToGo":
                    BatteryTimeToGo = decimal.Parse(value);
                    break;

                case "Voltage":
                    BatteryVoltage = decimal.Parse(value);
                    break;

            }
        }

        public override void Update(MqttApplicationMessage message)
        {
            UpdateValues(new VenusGxSensorData(message));
        }

        // battery properties

        public decimal BatteryConsumedAmphours { get; set; }
        public decimal BatteryCurrent { get; set; }
        public decimal BatteryPower { get; set; }
        public decimal BatterySoC { get; set; }
        public int BatteryState { get; set; }
        public decimal BatteryTemperature { get; set; }
        public decimal BatteryTimeToGo { get; set; }
        public decimal BatteryVoltage { get; set; }

    }

    public class VictronMqttPayload
    {
        public string value { get; set; }
    }
}

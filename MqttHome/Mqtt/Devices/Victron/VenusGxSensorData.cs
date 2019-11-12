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

        // GRID
        //N/7c386655e76b/system/0/Ac/Grid/DeviceType
        //N/7c386655e76b/system/0/Ac/Grid/L1/Power
        //N/7c386655e76b/system/0/Ac/Grid/L2/Power
        //N/7c386655e76b/system/0/Ac/Grid/L3/Power
        //N/7c386655e76b/system/0/Ac/Grid/NumberOfPhases
        //N/7c386655e76b/system/0/Ac/Grid/ProductId

        public VenusGxSensorData()
        {
            // set default sensor state here -- could just remain blank
        }

        public VenusGxSensorData(MqttApplicationMessage mqtt)
        {
            var message = Encoding.UTF8.GetString(mqtt.Payload);

            var topicArray = mqtt.Topic.Split('/');

            var value = JsonConvert.DeserializeObject<VictronMqttPayload>(message).value;

            try
            {
                switch (topicArray[5])
                {
                    case "Battery":
                        ParseBatteryData(topicArray, value);
                        break;

                    case "Grid":
                        ParseGridData(topicArray, value);
                        break;
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public override Dictionary<string, object> Update(MqttApplicationMessage message)
        {
            return UpdateValues(new VenusGxSensorData(message));
        }

        private void ParseBatteryData(string[] topicArray, string value)
        {
            switch (topicArray[6])
            {
                case "ConsumedAmphours":
                    BatteryConsumedAmphours = value == null ? 0 : decimal.Parse(value);
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
                    BatteryTimeToGo = value == null ? (decimal?)null : decimal.Parse(value);
                    break;

                case "Voltage":
                    BatteryVoltage = decimal.Parse(value);
                    break;

            }
        }
        private void ParseGridData(string[] topicArray, string value)
        {
            switch (string.Join("", topicArray.Skip(6).Take(2)))
            {
                case "L1Power":
                    GridL1Power = value == null ? 0m : decimal.Parse(value);
                    break;

                case "L2Power":
                    GridL2Power = value == null ? 0m : decimal.Parse(value);
                    break;

                case "L3Power":
                    GridL3Power = value == null ? 0m : decimal.Parse(value);
                    break;

                case "NumberOfPhases":
                    GridNumberOfPhases = value == null ? 0 : int.Parse(value);
                    break;

            }
        }

        // battery properties

        public decimal? BatteryConsumedAmphours { get; set; }
        public decimal? BatteryCurrent { get; set; }
        public decimal? BatteryPower { get; set; }
        public decimal? BatterySoC { get; set; }
        public int? BatteryState { get; set; }
        public decimal? BatteryTemperature { get; set; }
        public decimal? BatteryTimeToGo { get; set; }
        public decimal? BatteryVoltage { get; set; }

        // grid properties

        public decimal? GridL1Power { get; set; }
        public decimal? GridL2Power { get; set; }
        public decimal? GridL3Power { get; set; }
        public int? GridNumberOfPhases { get; set; }

    }

    public class VictronMqttPayload
    {
        public string value { get; set; }
    }
}

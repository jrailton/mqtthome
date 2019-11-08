using MqttHome.Mqtt;
using MqttHome.Mqtt.Devices;
using System;
using System.Collections.Generic;

namespace MqttHome
{
    public class Condition
    {
        public event EventHandler ConditionValueChanged;

        public string Id { get; set; }
        public string Device { get; set; }
        public string SensorValue { get; set; }

        public bool? ConditionValue { get; private set; } = null;

        public void CheckCondition(ISensorDevice<ISensorData> device, Dictionary<string, object> allSensorValues) {

            var array = SensorValue.Split(' ');
            var property = array[0];
            var comparer = array[1];
            var value = array[2];

            if (allSensorValues.ContainsKey(property)) {

                LastSensorValue = allSensorValues[property];
                bool newValue;

                switch (comparer) {
                    case ">":
                        newValue = ((decimal)allSensorValues[property] > decimal.Parse(value));
                        break;

                    case ">=":
                        newValue = ((decimal)allSensorValues[property] >= decimal.Parse(value));
                        break;

                    case "<=":
                        newValue = ((decimal)allSensorValues[property] <= decimal.Parse(value));
                        break;

                    case "<":
                        newValue = ((decimal)allSensorValues[property] < decimal.Parse(value));
                        break;

                    case "==":
                        newValue = allSensorValues[property].Equals(value);
                        break;

                    default:
                        throw new Exception($"Unrecognized condition comparer {comparer} on condition ID {Id}");
                }

                if (newValue != ConditionValue) {
                    ConditionValue = newValue;
                    ConditionValueChanged?.Invoke(this, null);
                }
            }
        }

        public object LastSensorValue { get; set; }
    }
}

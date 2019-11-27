using MqttHome.Mqtt;
using MqttHome.Mqtt.Devices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MqttHome
{
    public abstract class Condition
    {
        public event EventHandler ConditionValueChanged;

        public string Id { get; set; }
        public string Device { get; set; }
        public string Criteria { get; set; }
        public DateTime? ConditionValueUpdated { get; set; }

        public bool? ConditionValue { get; private set; } = null;

        public void CheckCondition(IMqttSensorDevice<ISensorData> device, Dictionary<string, object> allSensorValues)
        {
            var array = Criteria.Split(' ');
            var property = array[0];
            var comparer = array[1];
            dynamic value = array[2];

            // in case value is boolean
            bool temp;
            if (bool.TryParse(array[2], out temp))
                value = temp;

            if (allSensorValues.ContainsKey(property))
            {

                LastSensorValue = allSensorValues[property];
                bool newValue;

                switch (comparer)
                {
                    case ">":
                        newValue = ((dynamic)allSensorValues[property] > decimal.Parse(value));
                        break;

                    case ">=":
                        newValue = ((dynamic)allSensorValues[property] >= decimal.Parse(value));
                        break;

                    case "<=":
                        newValue = ((dynamic)allSensorValues[property] <= decimal.Parse(value));
                        break;

                    case "<":
                        newValue = ((dynamic)allSensorValues[property] < decimal.Parse(value));
                        break;

                    case "==":
                        newValue = allSensorValues[property].Equals(value);
                        break;

                    default:
                        throw new Exception($"Unrecognized condition comparer {comparer} on condition ID {Id}");
                }

                if (newValue != ConditionValue)
                {
                    ConditionValue = newValue;
                    ConditionValueUpdated = DateTime.Now;
                    ConditionValueChanged?.Invoke(this, null);
                }
            }
        }

        public object LastSensorValue { get; set; }

        public List<string> CheckProblems()
        {
            var problems = new List<string>();
            var array = Criteria?.Split(' ');

            if (array == null)
                problems.Add("SensorValue property is not specified");

            if (array.Length != 3)
                problems.Add("SensorValue property is invalid - should contain three parts: sensor data property name, comparer (==, >, >=, <, <=) and value");

            if (!new[] { "==", ">", ">=", "<", "<=" }.Contains(array[1]))
                problems.Add($"SensorValue comparer {array[1]} is not valid. It must be either ==, >, >=, < or <=");

            return problems;
        }
    }

    public interface ISwitchCondition { }

    public interface ISensorCondition { }

    public interface IPresenceCondition { }
}

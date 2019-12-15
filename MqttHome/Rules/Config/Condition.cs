using MqttHome.Mqtt;
using MqttHome.Mqtt.Devices;
using MqttHome.Presence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MqttHome
{
    public class Condition
    {
        public event EventHandler ConditionValueChanged;

        public string Id { get; set; }
        /// <summary>
        /// The device ID whose sensor data will be evaulated if criteria is specified
        /// </summary>
        public string Device { get; set; }
        /// <summary>
        /// The device sensor data property, comparer and value to be evaluated
        /// </summary>
        public string Criteria { get; set; }
        /// <summary>
        /// Optional. The AND/OR people present required to evaulate the condition as TRUE
        /// </summary>
        public string People { get; set; }
        public DateTime? ConditionValueUpdated { get; set; }

        public bool? ConditionValue { get; private set; } = null;

        private bool? _deviceCondition;
        public bool? DeviceCondition
        {
            get
            {
                // if no device is set, this will always return true
                return string.IsNullOrEmpty(Device) ?
                    true :
                    _deviceCondition;
            }
            private set
            {
                _deviceCondition = value;
            }
        }

        private bool? _peopleCondition;
        public bool? PeopleCondition
        {
            get
            {
                // if no device is set, this will always return true
                return string.IsNullOrEmpty(People) ?
                    true :
                    _peopleCondition;
            }
            private set
            {
                _peopleCondition = value;
            }
        }

        public void CheckDeviceCondition(IMqttSensorDevice<ISensorData> device, Dictionary<string, object> sensorValues)
        {
            var array = Criteria.Split(' ');
            var property = array[0];
            var comparer = array[1];
            dynamic value = array[2];

            // in case value is boolean
            bool temp;
            if (bool.TryParse(array[2], out temp))
                value = temp;

            if (sensorValues.ContainsKey(property))
            {

                LastSensorValue = sensorValues[property];
                bool newValue;

                switch (comparer)
                {
                    case ">":
                        newValue = ((dynamic)sensorValues[property] > double.Parse(value));
                        break;

                    case ">=":
                        newValue = ((dynamic)sensorValues[property] >= double.Parse(value));
                        break;

                    case "<=":
                        newValue = ((dynamic)sensorValues[property] <= double.Parse(value));
                        break;

                    case "<":
                        newValue = ((dynamic)sensorValues[property] < double.Parse(value));
                        break;

                    case "==":
                        newValue = sensorValues[property].Equals(value);
                        break;

                    default:
                        throw new Exception($"Unrecognized condition comparer {comparer} on condition ID {Id}");
                }

                if (newValue != DeviceCondition)
                {
                    DeviceCondition = newValue;
                    CheckAllConditions();
                }
            }
        }

        public void CheckPeopleCondition(IEnumerable<Person> people)
        {
            var parts = People.Split(' ');

            if (parts.Length % 2 == 0)
                throw new Exception($"CheckPeopleCondition :: Condition ID: {Id}. Failed - People condition must specify AND/OR operators if more than one person included");

            var result = true;
            for (int i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 0) // person
                {
                    result = people.SingleOrDefault(p => p.Id == parts[i])?.Present ?? false;
                }
                else // operator
                {
                    var op = parts[i].ToUpper();
                    if (op == "AND")
                    {
                        if (result == false)
                            break; // break out early because the previous condition already false
                    }
                    else if (op == "OR") {
                        if (result == true)
                            break; // break out early because previous condition already true
                    }
                }
            }

            if (PeopleCondition != result)
            {
                PeopleCondition = result;
                CheckAllConditions();
            }
        }

        private void CheckAllConditions()
        {
            var result = (DeviceCondition ?? false) && (PeopleCondition ?? false);
            if (result != ConditionValue)
            {
                ConditionValue = result;
                ConditionValueUpdated = DateTime.Now;
                ConditionValueChanged?.Invoke(this, null);
            }
        }

        public object LastSensorValue { get; set; }

        public List<string> CheckProblems()
        {
            var problems = new List<string>();

            var deviceSpec = !string.IsNullOrEmpty(Device);
            var critSpec = !string.IsNullOrEmpty(Criteria);
            var peopleSpec = !string.IsNullOrEmpty(People);

            // check both or neither device and criteria are specified
            if ((deviceSpec && !critSpec) || (critSpec && !deviceSpec))
                problems.Add("Although not required, device and criteria values are dependent on each other i.e. if one is specified, so must the other");

            // check that either people or device criteria specified
            if (!peopleSpec && !deviceSpec)
                problems.Add("Neither People nor Criteria properties are specified. Conditions must at least specify People or Criteria (or both) to be valid.");

            // check criteria -- only required if device id is specified
            if (deviceSpec)
            {
                var array = Criteria?.Split(' ');

                if (array == null)
                    problems.Add("Criteria property is not specified. If a device is specified, then criteria must also be specified.");

                if (array.Length != 3)
                    problems.Add("Criteria property is invalid - should contain three parts: device sensor data property name, comparer (==, >, >=, <, <=) and value");

                if (!new[] { "==", ">", ">=", "<", "<=" }.Contains(array[1]))
                    problems.Add($"Criteria comparer {array[1]} is not valid. It must be either ==, >, >=, < or <=");
            }

            if (peopleSpec)
            {
                var array = People?.Split(' ');

                // pita check -- TODO: :)
            }

            return problems;
        }
    }

    public interface ISwitchCondition { }

    public interface ISensorCondition { }

    public interface IPresenceCondition { }
}

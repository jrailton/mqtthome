using System;
using System.Collections.Generic;
using System.Linq;
using MQTTnet;

namespace MqttHome.Mqtt
{
    public abstract class SensorData : ISensorData
    {
        public abstract Dictionary<string, object> Update(MqttApplicationMessage message);

        protected Dictionary<string, object> UpdateValues(SensorData newValues)
        {
            try
            {
                var updated = new Dictionary<string, object>();

                foreach (var property in GetType().GetProperties())
                {
                    if (property.Name == "LoadWatts")
                        Console.Write("");

                    var newValue = property.GetValue(newValues);
                    if (!IsNullOrDefault(newValue) && !(property.GetValue(this)?.Equals(newValue) ?? false))
                    {
                        updated.Add(property.Name, newValue);
                        property.SetValue(this, newValue);
                    }
                }

                return updated;
            }
            catch (Exception err) {
                Console.WriteLine(err);
                return null;
            }
        }

        /// <summary>
        /// Generic ToDictionary which simply converts sensors properties to a dictionary of values -- and removes values that are null or default 
        /// (because sensors like ICC will accept many topics, not all of which will create values for all properties, and always return a full list 
        /// of sensor data which means some null or default values could be being written to the db unnecessarily) 
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            return GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(this))
                .Where(k => !IsNullOrDefault(k.Value))
                .ToDictionary(p => p.Key, p => p.Value);
        }

        protected bool IsNullOrDefault<T>(T argument)
        {
            // deal with normal scenarios
            if (argument == null) 
                return true;

            if (object.Equals(argument, default(T))) 
                return true;

            // deal with non-null nullables
            Type methodType = typeof(T);
            if (Nullable.GetUnderlyingType(methodType) != null) 
                return false;

            // deal with boxed value types
            //Type argumentType = argument.GetType();
            //if (argumentType.IsValueType && argumentType != methodType)
            //{
            //    object obj = Activator.CreateInstance(argument.GetType());
            //    return obj.Equals(argument);
            //}

            return false;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace InfluxDbLoader.Mqtt
{
    public class SensorData
    {
        /// <summary>
        /// Generic ToDictionary which simply converts sensors properties to a dictionary of values
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            return GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(this));
        }
    }
}

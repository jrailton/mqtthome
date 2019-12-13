using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MqttHome.Devices.Base;
using MqttHome.Mqtt;
using Newtonsoft.Json;

namespace MqttHomeWeb.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [Route("api/values/sensorvalues")]
        public string SensorValues(string names)
        {
            var output = new List<SensorValue>();
            var requests = names.Split(',');
            foreach (var request in requests)
            {
                var kv = request.Split('.', 2);
                var deviceId = kv[0];
                var valueName = kv[1];
                var device = Program.MqttHomeController.MqttDevices.Single(d => d.Id == deviceId);
                var sensor = device as ISensorDevice<ISensorData>;
                output.Add(new SensorValue(deviceId, valueName, sensor.SensorValues[valueName], device.Config.Widgets));
            }

            return JsonConvert.SerializeObject(output);
        }
    }

    public class SensorValue
    {
        public SensorValue(string deviceId, string valueName, object value, string valueFormatted)
        {
            DeviceId = deviceId;
            ValueName = valueName;
            Value = value;
            ValueFormatted = valueFormatted;
        }

        public string DeviceId;
        public object ValueName;
        public object Value;
        public string ValueFormatted;
        public string Id => $"{DeviceId}.{ValueName}";
    }
}
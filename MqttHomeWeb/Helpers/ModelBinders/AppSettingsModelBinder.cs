using Microsoft.AspNetCore.Mvc.ModelBinding;
using MqttHome.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MqttHomeWeb.Helpers.ModelBinders
{
    public class MqttBrokersModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var output = new List<MqttBroker>();

            var form = bindingContext.ActionContext.HttpContext.Request.Form;

            var mqttBrokers = form["mqtt-IpAddress"].ToArray();
            var mqttNames = form["mqtt-Name"].ToArray();
            var mqttPorts = form["mqtt-Port"].ToArray();

            for (var i = 0; i < mqttBrokers.Length; i++) {
                try
                {
                    if (!string.IsNullOrEmpty(mqttBrokers[i]))
                    {
                        output.Add(new MqttBroker
                        {
                            IpAddress = mqttBrokers[i],
                            Name = mqttNames[i] ?? "MQTT Broker " + i,
                            Port = int.Parse(mqttPorts[i] ?? "1883")
                        });
                    }
                }
                catch { 

                }
            }

            bindingContext.Result = ModelBindingResult.Success(output);

            return Task.CompletedTask;
        }
    }
}

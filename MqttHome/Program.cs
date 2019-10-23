using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using InfluxDbLoader.Influx;
using InfluxDbLoader.Mqtt;
using InfluxDbLoader.Mqtt.Devices;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics;
using System.Configuration;

namespace MqttHome
{
    public class MqttHomeController
    {
        public MqttCommunicator MqttCommunicator;
        public List<MqttDevice> MqttDevices;
        public InfluxCommunicator InfluxCommunicator;
        public List<string> MqttDeviceTopics;
        public bool Debug;

        public static void Log(string description)
        {
            Console.WriteLine($"{DateTime.Now.ToString("dd-MMM-yy HH:mm:ss")} {description}");
        }

        public MqttHomeController(bool debug, string influxUrl = "http://localhost:8086", string influxDatabase = "home_db")
        {
            try
            {
                Debug = debug;

                Log($"Connecting to Influx on '{influxUrl}' using database '{influxDatabase}'...");

                InfluxCommunicator = new InfluxCommunicator(new Uri(influxUrl), influxDatabase);

                MqttDevices = new List<MqttDevice>
                {
                    new ICC("icc"),
                    new SonoffTHDevice("th16_1"),
                    new SonoffTHDevice("th16_2"),
                    new SonoffPowR2Device("powr2_1"),
                    new SonoffPowR2Device("powr2_2"),
                    new SonoffPowR2Device("powr2_3"),
                    new SonoffPowR2Device("powr2_4"),
                    new SonoffPowR2Device("powr2_5"),
                    new SonoffGenericSwitchDevice("s26_2", MqttDeviceType.SonoffS26),
                };

                Log($"Adding {MqttDevices.Count} MQTT devices...");

                // this is a hack which needs more thought
                MqttDeviceTopics = MqttDevices.Select(d => d.StateTopic).ToList();
                MqttDeviceTopics.AddRange(MqttDevices.SelectMany(d => d.SensorTopics).ToList());
                MqttDeviceTopics.AddRange(MqttDevices.Select(d => d.CommandResponseTopic).ToList());
                MqttDeviceTopics = MqttDeviceTopics.Where(s => !string.IsNullOrEmpty(s)).ToList();

                var mqttBrokerIp = ConfigurationManager.AppSettings["mqttbroker_ip"] ?? "localhost";
                int mqttBrokerPort = int.Parse(ConfigurationManager.AppSettings["mqttbroker_port"] ?? "1883");

                Log($"Connecting to MQTT broker on {mqttBrokerIp}:{mqttBrokerPort}...");

                MqttCommunicator = new MqttCommunicator(mqttBrokerIp, mqttBrokerPort);
                MqttCommunicator.Start();

                Log($@"Started listening to {MqttDevices.Count} MQTT devices. Topic list is:
{string.Join(Environment.NewLine, MqttDeviceTopics)}");

            }
            catch (Exception err)
            {
                Log($"Exception in Main - {err.Message}");
            }
        }

        private void UpdateUI()
        {
            var whitespace = string.Empty;
            while (true)
            {
                var builder = new StringBuilder();

                foreach (var device in MqttDevices)
                {
                    builder.AppendLine($@"Class: {device.DeviceClass}, Type: {device.DeviceType}, ID: {device.Id}, State: {(device.PowerOn ? "On" : "Off")}");
                    if (device.SensorData != null)
                        builder.AppendLine($@"{string.Join(Environment.NewLine, device.SensorData.ToDictionary().Select(k => $"{k.Key}: {k.Value}"))}");
                }

                builder.AppendLine(@"

Press any key to exit...");

                Console.SetCursorPosition(0, 0);
                Console.Write(whitespace);

                var buffer = builder.ToString();

                Console.SetCursorPosition(0, 0);
                Console.Write(buffer);

                whitespace = Regex.Replace(buffer, @"[^\n]", " ");

                Thread.Sleep(1000);
            }
        }
    }
}

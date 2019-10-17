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

namespace InfluxDbLoader
{
    class Program
    {
        public static MqttCommunicator MqttCommunicator;
        public static List<MqttDevice> MqttDevices;
        public static InfluxCommunicator InfluxCommunicator;
        public static List<string> MqttDeviceTopics;
        public static bool Debug;

        public static void Log(string description)
        {
            Console.WriteLine($"{DateTime.Now.ToString("dd-MMM-yy HH:mm:ss")} {description}");
        }

        static void Main(string[] args)
        {
            try
            {
                Debug = args.Any(s => s.Equals("debug", StringComparison.CurrentCultureIgnoreCase));

                var influxUrl = ConfigurationManager.AppSettings["influxdb_url"] ?? "http://localhost:8086";
                var influxDatabase = ConfigurationManager.AppSettings["influxdb_database"] ?? "home_db";

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
                MqttDeviceTopics.AddRange(MqttDevices.Select(d => d.SensorTopic).ToList());
                MqttDeviceTopics = MqttDeviceTopics.Where(s => !string.IsNullOrEmpty(s)).ToList();

                var mqttBrokerIp = ConfigurationManager.AppSettings["mqttbroker_ip"] ?? "localhost";
                int mqttBrokerPort = int.Parse(ConfigurationManager.AppSettings["mqttbroker_port"] ?? "1883");

                Log($"Connecting to MQTT broker on {mqttBrokerIp}:{mqttBrokerPort}...");

                MqttCommunicator = new MqttCommunicator(mqttBrokerIp, mqttBrokerPort);
                MqttCommunicator.Start();

                Log($@"Started listening to {MqttDevices.Count} MQTT devices. Topic list is:
{string.Join(Environment.NewLine, MqttDeviceTopics)}");

                // only print on screen if in ui mode
                if (args.Any(s => s.Equals("ui", StringComparison.CurrentCultureIgnoreCase)))
                    Task.Run(() => UpdateUI());

            }
            catch (Exception err)
            {
                Log($"Exception in Main - {err.Message}");
            }

            // basically dont ever stop -- user will need to close the app ctrl+c
            while (true)
                Console.ReadKey(true);
        }

        private static void UpdateUI()
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

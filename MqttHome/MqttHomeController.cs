using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using MqttHome.Influx;
using MqttHome.Mqtt;
using MqttHome.Mqtt.Devices;

namespace MqttHome
{
    public class MqttHomeController
    {
        public MqttCommunicator MqttCommunicator;
        public IQueryable<MqttDevice> MqttDevices;
        public InfluxCommunicator InfluxCommunicator;
        public List<string> MqttDeviceTopics;
        public RuleEngine RuleEngine;
        public bool Debug;

        public ILog RuleLog;
        public ILog DeviceLog;
        public ILog GeneralLog;
        public ILog InfluxLog;
        public ILog MqttLog;

        public MqttHomeController(bool debug, ILog ruleLog, ILog deviceLog, ILog generalLog, ILog influxLog, ILog mqttLog, string mqttBrokerIp = "localhost", int mqttBrokerPort = 1883, string influxUrl = "http://localhost:8086", string influxDatabase = "home_db")
        {
            try
            {
                MqttLog = mqttLog;
                RuleLog = ruleLog;
                DeviceLog = deviceLog;
                GeneralLog = generalLog;
                InfluxLog = influxLog;

                Debug = debug;

                InfluxCommunicator = new InfluxCommunicator(influxLog, new Uri(influxUrl), influxDatabase);

                MqttDevices = new List<MqttDevice>
                {
                    new ICC(this, "icc"),
                    new SonoffTHDevice(this, "th16_1"),
                    new SonoffTHDevice(this, "th16_2"),
                    new SonoffPowR2Device(this, "powr2_1"),
                    new SonoffPowR2Device(this, "powr2_2"),
                    new SonoffPowR2Device(this, "powr2_3"),
                    new SonoffPowR2Device(this, "powr2_4"),
                    new SonoffPowR2Device(this, "powr2_5"),
                    new SonoffGenericSwitchDevice(this, "s26_2", MqttDeviceType.SonoffS26),
                }.AsQueryable();

                deviceLog.Debug($"Added {MqttDevices.Count()} MQTT devices...");

                // this is a hack which needs more thought
                MqttDeviceTopics = MqttDevices.SelectMany(d => d.AllTopics).Distinct().ToList();

                MqttCommunicator = new MqttCommunicator(this, mqttBrokerIp, mqttBrokerPort);

                RuleEngine = new RuleEngine(this);

                deviceLog.Debug($@"Started listening to {MqttDevices.Count()} MQTT devices. Topic list is:
{string.Join(Environment.NewLine, MqttDeviceTopics)}");

            }
            catch (Exception err)
            {
                generalLog.Error($"Exception in MqttHomeController.ctor - {err.Message}", err);
            }
        }

        public void Start()
        {
            MqttCommunicator.Start();
            RuleEngine.Start();
        }

        private void UpdateUI()
        {
            var whitespace = string.Empty;
            while (true)
            {
                var builder = new StringBuilder();

                foreach (var device in MqttDevices)
                {
                    builder.AppendLine($@"Class: {device.DeviceClass}, Type: {device.DeviceType}, ID: {device.Id}, State: {(device.PowerOn ? "On" : $"Off ({device.PowerOffTime?.ToString("HH:mm:ss") ?? "n/a"})")}");
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

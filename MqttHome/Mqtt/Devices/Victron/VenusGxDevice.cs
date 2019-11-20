using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;

namespace MqttHome.Mqtt.Devices.Victron
{
    public class VenusGxDevice : MqttSensorDevice<VenusGxSensorData>, ISensorDevice<ISensorData>
    {
        private string _venusGxMqttServerAddress;
        private int _venusGxMqttServerPort;
        private Timer _timer;

        public VenusGxDevice(MqttHomeController controller, string id, string friendlyName, params string[] config) : base(controller, id, friendlyName, DeviceType.VictronCCGX, config)
        {
            SensorTopics = new List<string> {
                $"N/{id}/system/0/Dc/Battery/#",
                $"N/{id}/system/0/Ac/Grid/#"
            };

            if (config == null || config.Length !=2)
                controller.DeviceLog.Error($"VenusGxDevice.ctor Error :: Device ID {id} ({friendlyName}) - Missing config parameters (for Venus GX Mqtt Server). Should contain two: ip address and port number");

            _venusGxMqttServerAddress = config[0];
            _venusGxMqttServerPort = int.Parse(config[1]);

            // setup the publish command to keep venus gx device publishing alive
            // setup timer to send publish command every 30 seconds
            _timer = new Timer((state) => {
                PublishKeepAlive();
            }, null, 0, 30000);
        }

        private void PublishKeepAlive() {
            try
            {
                // Create a new MQTT client.
                var factory = new MqttFactory();
                var client = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithClientId($"venusgx-deviceid-{Id}")
                    .WithTcpServer(_venusGxMqttServerAddress, _venusGxMqttServerPort)
                    //.WithCredentials("jimbo", "27Collins")
                    //.WithTls()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                    .WithCleanSession()
                    .Build();

                // connect
                var connectResult = client.ConnectAsync(options, CancellationToken.None).Result;

                if (connectResult.ResultCode != MQTTnet.Client.Connecting.MqttClientConnectResultCode.Success)
                {
                    Controller.DeviceLog.Error($"Device ID {Id} ({FriendlyName}) :: Failed to connect to Venus GX MQTT - {connectResult.ResultCode}");
                }
                else
                {
                    var publishResult = client.PublishAsync($"R/{Id}/system/0/Serial").Result;

                    if (publishResult.ReasonCode != MqttClientPublishReasonCode.Success)
                        Controller.DeviceLog.Error($"Device ID {Id} ({FriendlyName}) :: Failed to publish to Venus GX MQTT - {connectResult.ResultCode}");
                }
            }
            catch (Exception err) {
                Controller.DeviceLog.Error($"Device ID {Id} ({FriendlyName}) :: Keepalive command for {Id} failed. {err.Message}", err);
            }
        }

        public override DeviceType DeviceType => DeviceType.VictronCCGX;
        public override DeviceClass DeviceClass => DeviceClass.Sensor;
    }
}

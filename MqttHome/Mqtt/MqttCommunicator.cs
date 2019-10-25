using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;

namespace MqttHome.Mqtt
{
    public class MqttCommunicator
    {
        private IMqttClient _mqttClient;
        private IMqttClientOptions _mqttOptions;
        private Queue<MqttCommand> _commandQueue;
        private MqttHomeController _controller;

        public bool Connected { get; private set; }
        public string TopicFilter { get; private set; }
        public string BrokerIpAddress { get; private set; }
        public int BrokerPort { get; private set; }

        public MqttCommunicator(MqttHomeController controller, string brokerIpAddress = "localhost", int brokerPort = 1883)
        {
            BrokerIpAddress = brokerIpAddress;
            BrokerPort = brokerPort;

            _controller = controller;

            // Create a new MQTT client.
            var factory = new MqttFactory();

            _mqttClient = factory.CreateMqttClient();

            // attach disconnected/failure to connect event handler
            _mqttClient.UseDisconnectedHandler(MqttClientDisconnectedEvent);
            _mqttClient.UseApplicationMessageReceivedHandler(MqttClientReceivedMessageEvent);
            _mqttClient.UseConnectedHandler(MqttClientConnectedEvent);

            // Create TCP based options using the builder.
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("mqtt-logger")
                .WithTcpServer(brokerIpAddress, brokerPort)
                //.WithCredentials("jimbo", "27Collins")
                //.WithTls()
                .WithCleanSession()
                .Build();

            _commandQueue = new Queue<MqttCommand>();
        }

        public async void PublishCommand(MqttCommand command)
        {
            try
            {
                if (Connected)
                {
                    _controller.MqttLog.Debug($"PublishCommand :: Publishing - Device: {command.DeviceId}, Topic: {command.Topic}, Payload: {Encoding.UTF8.GetString(command.Payload)}");
                    var x = _mqttClient.PublishAsync(command.Topic, command.Payload);
                }
                else
                {
                    _controller.MqttLog.Debug($"PublishCommand :: Enqueuing - Device: {command.DeviceId}, Topic: {command.Topic}, Payload: {Encoding.UTF8.GetString(command.Payload)}");

                    _commandQueue.Enqueue(command);
                }
            }
            catch (Exception err)
            {
                _controller.MqttLog.Debug($"PublishCommand :: Failed - {err.Message}");
            }
        }

        public void Start(string topicFilter = "#")
        {
            _controller.MqttLog.Debug($"Connecting to MQTT broker on {BrokerIpAddress}:{BrokerPort}...");

            TopicFilter = topicFilter;

            _mqttClient.ConnectAsync(_mqttOptions, CancellationToken.None);
        }

        private async Task MqttClientConnectedEvent(MqttClientConnectedEventArgs e)
        {
            Connected = true;

            _controller.MqttLog.Debug("MqttClientConnectedEvent");

            // send commands from queue
            while (_commandQueue.Count > 0)
                PublishCommand(_commandQueue.Dequeue());

            // build topics list from mqtt devices
            var topicBuilder = new TopicFilterBuilder().WithTopic(TopicFilter);

            var topicFilter = topicBuilder.Build();

            // Subscribe to topics
            await _mqttClient.SubscribeAsync(topicFilter);

            _controller.MqttLog.Debug($"MqttClientConnectedEvent :: SUBSCRIBED TO {topicFilter.Topic}");
        }

        private async Task MqttClientReceivedMessageEvent(MqttApplicationMessageReceivedEventArgs e)
        {
            if (_controller.MqttDeviceTopics.Contains(e.ApplicationMessage.Topic))
            {
//                _controller.MqttLog.Debug($@"MqttClientReceivedMessageEvent
//----------------------------------------
//+ Topic = {e.ApplicationMessage.Topic}
//+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}
//+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}
//+ Retain = {e.ApplicationMessage.Retain}
//========================================
//");

                try
                {
                    // states
                    foreach (var device in _controller.MqttDevices.Where(d => d.IsSubscribedToStateTopic(e.ApplicationMessage.Topic)))
                        device.ParseStatePayload(e.ApplicationMessage);

                    // states
                    foreach (var device in _controller.MqttDevices.Where(d => d.IsSubscribedToCommandResponseTopic(e.ApplicationMessage.Topic)))
                        device.ParseCommandResponsePayload(e.ApplicationMessage);

                    // sensors
                    foreach (var device in _controller.MqttDevices.Where(d => d.IsSubscribedToSensorTopic(e.ApplicationMessage.Topic)))
                        device.SensorData.Update(e.ApplicationMessage);
                }
                catch (Exception err)
                {
                    _controller.MqttLog.Error($"MqttClientReceivedMessageEvent :: Error - {err.Message}", err);
                }

            }
            //Task.Run(() => _mqttClient.PublishAsync("hello/world"));
        }

        private async Task MqttClientDisconnectedEvent(MqttClientDisconnectedEventArgs mqttClientDisconnectedEventArgs)
        {
            Connected = false;

            _controller.MqttLog.Warn("MqttClientDisconnectedEvent");

            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await _mqttClient.ConnectAsync(_mqttOptions, CancellationToken.None);
            }
            catch(Exception err)
            {
                _controller.MqttLog.Error($"MqttClientDisconnectedEvent :: Reconnect failed. {err.Message}", err);
            }
        }
    }
}

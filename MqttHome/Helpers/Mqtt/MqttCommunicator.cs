using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MqttHome.Mqtt.BrokerCommunicator;
using MqttHome.Mqtt.Devices;
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
        public MqttBroker Broker { get; private set; }
        public int BrokerPort { get; private set; }

        public MqttCommunicator(MqttHomeController controller, MqttBroker broker)
        {
            Broker = broker;

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
                .WithTcpServer(broker.IpAddress, broker.Port)
                //.WithCredentials("jimbo", "27Collins")
                //.WithTls()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
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

                    // cater for possible empty payload
                    if (command.Payload != null && command.Payload.Length > 0)
                    {
                        await _mqttClient.PublishAsync(command.Topic, command.Payload);
                    }
                    else {
                        await _mqttClient.PublishAsync(command.Topic);
                    }
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
            _controller.MqttLog.Debug($"Connecting to MQTT broker {Broker.Name} on {Broker.IpAddress}:{Broker.Port}...");

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

        private bool DeviceSubscribedToTopic(string topic, string subscription)
        {
            return DeviceSubscribedToTopic(topic, new List<string> { subscription });
        }
        private bool DeviceSubscribedToTopic(string topic, List<string> subscription)
        {
            foreach (var s in subscription)
            {
                var sub = s;
                sub = sub.Replace(@"#", @".*");
                sub = sub.Replace(@"+", @"[^\/]");

                if (Regex.IsMatch(topic, sub))
                    return true;
            }

            return false;
        }

        private async Task MqttClientReceivedMessageEvent(MqttApplicationMessageReceivedEventArgs e)
        {
            if (DeviceSubscribedToTopic(e.ApplicationMessage.Topic, _controller.MqttDeviceTopics))
            {
                try
                {
                    // switch devices -- they require commandresponse and state topic routing
                    foreach (ISwitchDevice device in _controller.MqttDevices.Where(d => d is ISwitchDevice))
                    {
                        // state
                        if (DeviceSubscribedToTopic(e.ApplicationMessage.Topic, device.StateTopic))
                        {
                            try
                            {
                                device.LastCommunication = DateTime.Now;
                                device.ParseStatePayload(e.ApplicationMessage);
                            }
                            catch (Exception err)
                            {
                                _controller.DeviceLog.Error($"MqttClientReceivedMessageEvent :: Device: {device.Id} - Failed to ParseStatePayload. {err.Message}", err);
                            }
                        }

                        // command response
                        if (DeviceSubscribedToTopic(e.ApplicationMessage.Topic, device.CommandResponseTopic))
                        {
                            try
                            {
                                device.LastCommunication = DateTime.Now;
                                device.ParseCommandResponsePayload(e.ApplicationMessage);
                            }
                            catch (Exception err)
                            {
                                _controller.DeviceLog.Error($"MqttClientReceivedMessageEvent :: Device: {device.Id} - Failed to ParseCommandResponsePayload. {err.Message}", err);
                            }
                        }
                    }

                    // sensor devices
                    foreach (IMqttSensorDevice<ISensorData> device in _controller.MqttDevices.Where(d => d is IMqttSensorDevice<ISensorData>))
                    {
                        if (DeviceSubscribedToTopic(e.ApplicationMessage.Topic, device.SensorTopics))
                        {
                            try
                            {
                                device.LastCommunication = DateTime.Now;
                                device.ParseSensorPayload(e.ApplicationMessage);
                            }
                            catch (Exception err) {
                                _controller.DeviceLog.Error($"MqttClientReceivedMessageEvent :: Device: {device.Id} - Failed to ParseSensorPayload. {err.Message}", err);
                            }
                        }
                    }

                }
                catch (Exception err)
                {
                    _controller.MqttLog.Error($"MqttClientReceivedMessageEvent :: Error - {err.Message}", err);
                }

            }
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
            catch (Exception err)
            {
                _controller.MqttLog.Error($"MqttClientDisconnectedEvent :: Reconnect failed. {err.Message}", err);
            }
        }
    }
}

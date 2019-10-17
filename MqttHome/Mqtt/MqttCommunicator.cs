using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;

namespace InfluxDbLoader.Mqtt
{
    public class MqttCommunicator
    {
        private IMqttClient _mqttClient;
        private IMqttClientOptions _mqttOptions;
        private Queue<MqttCommand> _commandQueue;
        public bool Connected { get; private set; }
        public string TopicFilter { get; private set; }

        public MqttCommunicator(string brokerIpAddress = "localhost", int port = 1883)
        {
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
                .WithTcpServer(brokerIpAddress, port)
                //.WithCredentials("jimbo", "27Collins")
                //.WithTls()
                .WithCleanSession()
                .Build();

            _commandQueue = new Queue<MqttCommand>();
        }

        public async void PublishCommand(MqttCommand command)
        {
            if (Connected)
            {
                var x = _mqttClient.PublishAsync(command.Topic, command.Payload);
            }

            _commandQueue.Enqueue(command);
        }

        public void Start(string topicFilter = "#")
        {
            TopicFilter = topicFilter;

            _mqttClient.ConnectAsync(_mqttOptions, CancellationToken.None);
        }

        private async Task MqttClientConnectedEvent(MqttClientConnectedEventArgs e)
        {
            Connected = true;

            if (Program.Debug)
                Console.WriteLine("### CONNECTED WITH SERVER ###");

            // send commands from queue
            while (_commandQueue.Count > 0)
                PublishCommand(_commandQueue.Dequeue());

            // build topics list from mqtt devices
            var topicBuilder = new TopicFilterBuilder().WithTopic(TopicFilter);

            var topicFilter = topicBuilder.Build();

            // Subscribe to topics
            await _mqttClient.SubscribeAsync(topicFilter);

            if (Program.Debug)
                Console.WriteLine($"### SUBSCRIBED TO {topicFilter.Topic} ###");
        }

        private async Task MqttClientReceivedMessageEvent(MqttApplicationMessageReceivedEventArgs e)
        {
            if (Program.MqttDeviceTopics.Contains(e.ApplicationMessage.Topic))
            {

                if (Program.Debug)
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();
                }

                try
                {
                    // states
                    foreach (var device in Program.MqttDevices.Where(d => d.IsSubscribedToStateTopic(e.ApplicationMessage.Topic)))
                        device.ParseStatePayload(e.ApplicationMessage);

                    // sensors
                    foreach (var device in Program.MqttDevices.Where(d => d.IsSubscribedToSensorTopic(e.ApplicationMessage.Topic)))
                        device.ParseSensorPayload(e.ApplicationMessage);
                }
                catch (Exception err)
                {
                    if (Program.Debug)
                        Console.WriteLine(err.Message);
                }

            }
            //Task.Run(() => _mqttClient.PublishAsync("hello/world"));
        }

        private async Task MqttClientDisconnectedEvent(MqttClientDisconnectedEventArgs mqttClientDisconnectedEventArgs)
        {
            Connected = false;

            if (Program.Debug)
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await _mqttClient.ConnectAsync(_mqttOptions, CancellationToken.None);
            }
            catch
            {
                if (Program.Debug)
                    Console.WriteLine("### RECONNECTING FAILED ###");
            }
        }
    }
}

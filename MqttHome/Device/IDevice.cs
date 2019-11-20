using MqttHome.Mqtt;

namespace MqttHome
{
    public interface IDevice
    {
        public MqttHomeController Controller { get; }

        public string FriendlyName { get; }

        public string Id { get; }

        public DeviceType DeviceType { get; }
        public DeviceClass DeviceClass { get; }
    }
}

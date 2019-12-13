using MqttHome.Config;

namespace MqttHome
{
    public abstract class Device : IDevice
    {
        public Device(MqttHomeController controller, Config.Device config) {
            Controller = controller;
            Id = config.Id;
            FriendlyName = config.FriendlyName;
            Config = config;

            Controller.DeviceLog.Debug($"Adding {DeviceType} {DeviceClass} device {config.Id}");

        }

        public MqttHomeController Controller { get; protected set; }

        public string FriendlyName { get; protected set; }

        public string Id { get; set; }

        public virtual DeviceType DeviceType { get; set; }

        public virtual DeviceClass DeviceClass { get; set; }

        public Config.Device Config { get; set; }
    }
}

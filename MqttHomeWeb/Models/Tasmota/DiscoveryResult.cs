namespace MqttHomeWeb.Models.Tasmota
{
    public class DiscoveryResult
    {
        public DiscoveryResult(string ipaddress)
        {
            IpAddress = ipaddress;
        }

        public string IpAddress;
        public bool Available;
        public bool IsTasmota;
        public string Errors;
        public string Timezone;
        public string FriendlyName;
        public TasmotaResponse TasmotaResponse;
    }
}
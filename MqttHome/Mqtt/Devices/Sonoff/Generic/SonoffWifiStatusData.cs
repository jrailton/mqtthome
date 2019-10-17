namespace InfluxDbLoader.Mqtt
{
    public class SonoffWifiStatusData
    {
        public int AP { get; set; }
        public string SSId { get; set; }
        public string BSSId { get; set; }
        public int Channel { get; set; }
        public int RSSI { get; set; }
        public int LinkCount { get; set; }
        public string Downtime { get; set; }
    }
}

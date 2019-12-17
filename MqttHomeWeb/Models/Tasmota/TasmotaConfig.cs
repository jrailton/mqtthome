using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MqttHomeWeb.Models.Tasmota
{
    public class TasmotaConfig
    {
        public Status Status { get; set; }
        public Statusprm StatusPRM { get; set; }
        public Statusfwr StatusFWR { get; set; }
        public Statuslog StatusLOG { get; set; }
        public Statusmem StatusMEM { get; set; }
        public Statusnet StatusNET { get; set; }
        public Statusmqt StatusMQT { get; set; }
        public Statustim StatusTIM { get; set; }
        public Statuspth StatusPTH { get; set; }
        public Statussns StatusSNS { get; set; }
        public Statussts StatusSTS { get; set; }
    }

    public class Status
    {
        public int Module { get; set; }
        public string[] FriendlyName { get; set; }
        public string Topic { get; set; }
        public string ButtonTopic { get; set; }
        public int Power { get; set; }
        public int PowerOnState { get; set; }
        public int LedState { get; set; }
        public string LedMask { get; set; }
        public int SaveData { get; set; }
        public int SaveState { get; set; }
        public string SwitchTopic { get; set; }
        public int[] SwitchMode { get; set; }
        public int ButtonRetain { get; set; }
        public int SwitchRetain { get; set; }
        public int SensorRetain { get; set; }
        public int PowerRetain { get; set; }
    }

    public class Statusprm
    {
        public int Baudrate { get; set; }
        public string GroupTopic { get; set; }
        public string OtaUrl { get; set; }
        public string RestartReason { get; set; }
        public string Uptime { get; set; }
        public DateTime StartupUTC { get; set; }
        public int Sleep { get; set; }
        public int CfgHolder { get; set; }
        public int BootCount { get; set; }
        public int SaveCount { get; set; }
        public string SaveAddress { get; set; }
    }

    public class Statusfwr
    {
        public string Version { get; set; }
        public DateTime BuildDateTime { get; set; }
        public int Boot { get; set; }
        public string Core { get; set; }
        public string SDK { get; set; }
    }

    public class Statuslog
    {
        public int SerialLog { get; set; }
        public int WebLog { get; set; }
        public int SysLog { get; set; }
        public string LogHost { get; set; }
        public int LogPort { get; set; }
        public string[] SSId { get; set; }
        public int TelePeriod { get; set; }
        public string Resolution { get; set; }
        public string[] SetOption { get; set; }
    }

    public class Statusmem
    {
        public int ProgramSize { get; set; }
        public int Free { get; set; }
        public int Heap { get; set; }
        public int ProgramFlashSize { get; set; }
        public int FlashSize { get; set; }
        public string FlashChipId { get; set; }
        public int FlashMode { get; set; }
        public string[] Features { get; set; }
    }

    public class Statusnet
    {
        public string Hostname { get; set; }
        public string IPAddress { get; set; }
        public string Gateway { get; set; }
        public string Subnetmask { get; set; }
        public string DNSServer { get; set; }
        public string Mac { get; set; }
        public int Webserver { get; set; }
        public int WifiConfig { get; set; }
    }

    public class Statusmqt
    {
        public string MqttHost { get; set; }
        public int MqttPort { get; set; }
        public string MqttClientMask { get; set; }
        public string MqttClient { get; set; }
        public string MqttUser { get; set; }
        public int MqttCount { get; set; }
        public int MAX_PACKET_SIZE { get; set; }
        public int KEEPALIVE { get; set; }
    }

    public class Statustim
    {
        public string UTC { get; set; }
        public string Local { get; set; }
        public string StartDST { get; set; }
        public string EndDST { get; set; }
        public string Timezone { get; set; }
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
    }

    public class Statuspth
    {
        public int PowerDelta { get; set; }
        public int PowerLow { get; set; }
        public int PowerHigh { get; set; }
        public int VoltageLow { get; set; }
        public int VoltageHigh { get; set; }
        public int CurrentLow { get; set; }
        public int CurrentHigh { get; set; }
    }

    public class Statussns
    {
        public DateTime Time { get; set; }
        public ENERGY ENERGY { get; set; }
    }

    public class ENERGY
    {
        public DateTime TotalStartTime { get; set; }
        public float Total { get; set; }
        public float Yesterday { get; set; }
        public float Today { get; set; }
        public int Power { get; set; }
        public int ApparentPower { get; set; }
        public int ReactivePower { get; set; }
        public float Factor { get; set; }
        public int Voltage { get; set; }
        public float Current { get; set; }
    }

    public class Statussts
    {
        public DateTime Time { get; set; }
        public string Uptime { get; set; }
        public int Heap { get; set; }
        public string SleepMode { get; set; }
        public int Sleep { get; set; }
        public int LoadAvg { get; set; }
        public string POWER { get; set; }
        public Wifi Wifi { get; set; }
    }

    public class Wifi
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

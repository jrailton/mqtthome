namespace MqttHome.Devices.Serial.Pylontech
{
    public class PPGetVersionInfo : PylonPacket
    {
        public PPGetVersionInfo() : base()
        {
            CID1 = 0x46;
            CID2 = 0x4F;
        }
    }
}

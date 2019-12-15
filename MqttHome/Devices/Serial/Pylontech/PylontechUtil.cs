using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace MqttHome.Devices.Serial.Pylontech
{
    public class PylontechUtil
    {
        public static void GetInfo()
        {
            var pc = new PylonCom("COM5");

            //# for adr in range(0,255):
            //# ppIn = pylonpacket.PPGetVersionInfo()
            //# ppIn.ADR=adr
            //# ppOut=pc.GetReply(ppIn, pylonpacket.PPVersionInfo)
            //#if ppOut: print("Get protocol version reply:",ppOut)
            //# return

            //# ppIn=pylonpacket.PPGetManufacturerInfo()
            //# print("Get manufacturer info:",ppIn)
            //# ppOut=pc.GetReply(ppIn, pylonpacket.PPManufacturerInfo)
            //# print("Get manufacturer info reply:",ppOut)

            var gspIn = new PPGetSystemParameter();
            Console.WriteLine($"Get system parameter: {gspIn}");

            var gspOut = pc.GetReply<PPSystemParameter>(gspIn);
            Console.WriteLine($"Reply system parameter: {gspOut}");

            var gsnIn = new PPGetSeriesNumber();
            gsnIn.Command = 0x02;
            Console.WriteLine($"Get series number: {gsnIn}");

            var gsnOut = pc.GetReply<PPSeriesNumber>(gsnIn);
            Console.WriteLine($"Reply series number: {gsnOut}");

            var gaiIn = new PPGetAlarmInformation();
            gaiIn.Command = 0x02;
            Console.WriteLine("Get alarm info:", gaiIn);
            var gaiOut = pc.GetReply<PPAlarmInformation>(gaiIn);
            Console.WriteLine($"Get alarm info reply: {gaiOut}");

            while (true)
            {
                var adrs = new byte[] { Convert.ToByte(2), Convert.ToByte(3) };
                foreach (var adr in adrs)
                {
                    Console.WriteLine($"Conectiong to addr {adr}");
                    var cmiIn = new PPGetChargeManagementInformation();
                    cmiIn.Command = adr;
                    cmiIn.ADR = adr;
                    //# print("Get charge info:",ppIn)
                    var cmiOut = pc.GetReply<PPChargeManagementInformation>(cmiIn);
                    Console.WriteLine($"Get charge info reply: {cmiOut}");

                    var gavIn = new PPGetAnalogValue();
                    gavIn.Command = adr;
                    gavIn.ADR = adr;

                    //# print("Get analog:",ppIn)
                    var gavOut = pc.GetReply<PPAnalogValue>(gavIn);
                    Console.WriteLine($"Get analog reply: {gavOut}");
                }
                Console.WriteLine("");
                Thread.Sleep(2000);
            }

            //ppIn = pylonpacket.PPTurnOff()
            //ppIn.Command = 0x02
            //print("Turn off:", ppIn)
            //ppOut = pc.GetReply(ppIn, pylonpacket.PPTurnOffReply)
            //print("Turn off reply:", ppOut)
        }
    }
}





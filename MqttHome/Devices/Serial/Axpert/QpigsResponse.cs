using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MqttHome.Devices.Serial.Axpert
{
    public class QpigsResponse
    {
        public float GridVoltage { get; set; }
        public float GridFrequency { get; set; }
        public float OutputVoltage { get; set; }
        public float OutputFrequency { get; set; }
        public int OutputVA { get; set; }
        public int OutputWatts { get; set; }

        //HHH is the maximum one of W% or VA% value.
        //VA% is a percent of apparent power.
        //W% is a percent of active power. The unit is %.
        public int OutputLoadPcnt { get; set; }

        public int BusVoltage { get; set; }
        public float BatteryVoltage { get; set; }
        public float BatteryChargingCurrent { get; set; }
        public float BatterySoc { get; set; }
        public int InverterTemp { get; set; }
        public int PvInputCurrent { get; set; }
        public int PvInputVoltage { get; set; }
        public float BatteryVoltage2 { get; set; }
        public int BatteryDischargeCurrent { get; set; }
        public DeviceStatus DeviceStatus { get; set; }
        public int InverterFanCentiVolts { get; set; }
        public int EepromVersion { get; set; }
        public int PvWatts { get; set; }
        public DeviceStatus2 DeviceStatus2 { get; set; }

        public QpigsResponse(string input) {
            var vals = input.Split(' ');

            GridVoltage = float.Parse(vals[0]);
            GridFrequency = float.Parse(vals[1]);
            OutputVoltage = float.Parse(vals[2]);
            OutputFrequency = float.Parse(vals[3]);
            OutputVA = int.Parse(vals[4]);
            OutputWatts = int.Parse(vals[5]);
            OutputLoadPcnt = int.Parse(vals[6]);
            BusVoltage = int.Parse(vals[7]);
            BatteryVoltage = float.Parse(vals[8]);
            BatteryChargingCurrent = float.Parse(vals[9]);
            BatterySoc = float.Parse(vals[10]);
            InverterTemp = int.Parse(vals[11]);
            PvInputCurrent = int.Parse(vals[12]);
            PvInputVoltage = int.Parse(vals[13]);
            BatteryVoltage2 = float.Parse(vals[14]);
            BatteryDischargeCurrent = int.Parse(vals[15]);
            DeviceStatus = new DeviceStatus(vals[16]);
            InverterFanCentiVolts = int.Parse(vals[17]);
            EepromVersion = int.Parse(vals[18]);
            PvWatts = int.Parse(vals[19]);
            DeviceStatus2 = new DeviceStatus2(vals[20]);
        }

        //1. (Start byte N: the integer from 0 to 9
        //2. N1N2N3.N5 Grid voltage V
        //3. N7N8.N10 Grid frequency Hz
        //4. N12N13N14.N16 AC output voltage V
        //5. N18N19.N21 AC output frequency Hz
        //6. N23N24N25N26 AC output apparent power VA
        //7. N28N29N30N31 AC output active power W
        //8. N33N34N35 Output load percent
        //9. N37N38N39 BUS voltage V
        //10. N41N42.N44N45 Battery voltage V
        //11. N47N48N49 Battery charging current A
        //12. N51N52N53 Battery capacity %
        //13. N55N56N57N58 Inverter heat sink temperature The unit is ℃（NTC A/D value for 1~3KVA ）
        //14. N60N61N62N63 PV Input current 1A
        //15. N65N66N67.N69 PV Input voltage 1 V
        //16. N71N72.N74N75 Battery voltage from SCC 1 V
        //17. N77N78N79N80N81 Battery discharge current A
        //18. b83b84b85b86b87b88b89b90 Device status
        //19. N92N93 Battery voltage offset for fans on 10mV
        //20. N95N96 EEPROM version
        //21. N98N99N100N101N102 PV Charging power 1 Watt
        //22. b104b105b106 Device status
    }

    public class DeviceStatus2
    {
        public DeviceStatus2(string input)
        {
            //B104: flag for charging to floating mode
            //b105: Switch On
            //b106: reserved

            // parse an array of booleans out of the input string of 1/0 integers
            var vals = input.ToCharArray().Select(c => (int.Parse(c.ToString()) == 1)).ToArray();

            FloatingMode = vals[0];
            SwitchOn = vals[1];
            Reserved = vals[2];
        }

        public bool FloatingMode { get; set; }
        public bool SwitchOn { get; set; }
        public bool Reserved { get; set; }
    }

    public class DeviceStatus
    {

        public DeviceStatus(string input)
        {
            //b83: add SBU priority version, 1: yes, 0: no
            //b84: configuration status: 1: Change 0: unchanged
            //b85: SCC firmware version 1: Updated 0: unchanged
            //b86: Load status: 0: Load off 1:Load on
            //b87: battery voltage to steady while charging
            //b88: Charging status(Charging on/off)
            //b89: Charging status(SCC1 charging on/off)
            //B90: Charging status(AC charging on/off)

            //Example of b88b89b90:
            //000: Do nothing
            //110: Charging on with SCC1 charge on
            //101: Charging on with AC charge on
            //111: Charging on with SCC1 and AC charge on

            // parse an array of booleans out of the input string of 1/0 integers
            var vals = input.ToCharArray().Select(c => (int.Parse(c.ToString()) == 1)).ToArray();

            SBUPriority = vals[0];
            ConfigChanged = vals[1];
            FirmwareVersionChanged = vals[2];
            LoadOn = vals[3];
            WTF = vals[4];
            Charging = vals[5];
            PvCharging = vals[6];
            AcCharging = vals[7];
        }

        public bool SBUPriority { get; set; }
        public bool ConfigChanged { get; set; }
        public bool FirmwareVersionChanged { get; set; }
        public bool LoadOn { get; set; }
        public bool WTF { get; set; }
        public bool Charging { get; set; }
        public bool PvCharging { get; set; }
        public bool AcCharging { get; set; }

    }
}

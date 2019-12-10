using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public class PPVersionInfo : PylonPacket { }

    public class PPGetManufacturerInfo : PylonPacket
    {
        public PPGetManufacturerInfo() : base()
        {
            CID1 = 0x46;
            CID2 = 0x51;
        }
    }

    public class PPManufacturerInfo : PylonPacket
    {
        public string DeviceName => Encoding.UTF8.GetString(INFO[0..10]).Trim();
        public byte[] SoftwareVersion => INFO[10..13];
        public string ManufacturerName => Encoding.UTF8.GetString(INFO[12..]).Trim();

        public override string ToString()
        {
            return $"{base.ToString()}DeviceName: {DeviceName}, SoftwareVersion {SoftwareVersion[0]}.{SoftwareVersion[1]}, ManufacturerName: {ManufacturerName}";
        }
    }

    public class PPGetAnalogValue : PylonPacket
    {
        public PPGetAnalogValue() : base()
        {
            INFO = new byte[1];
            LENGTH = 0x02;
            CID1 = 0x46;
            CID2 = 0x42;
        }

        public byte Command
        {
            get => INFO[0];
            set => INFO[0] = value;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Command: {Command}";
        }
    }

    public class PPAnalogValue : PylonPacket
    {
        public PPAnalogValue() : base()
        {
        }

        public double[] CellVoltages { get; private set; }
        public double[] Temperatures { get; private set; }
        public int CellsCount => INFO[2];
        public int TemperaturesCount => INFO[(CellsCount * 2) + 3];
        public double TotalCurrent => GetInt2Complement(-11) / 10.0;
        public double TotalVoltage => GetInt2(-9) / 1000.0;
        public double RemainingCapacity => GetInt2(-7) / 1000.0;
        public byte Quantity => INFO[INFO.Length - 5];
        public double TotalCapacity => GetInt2(-4) / 1000.0;
        public double Cycles => GetInt2(-2);

        public override void PostParse()
        {
            Console.WriteLine($"Post processing parsed data {string.Join("", INFO.Select(b => b.ToString("X2")))}");

            CellVoltages = new double[CellsCount];
            Temperatures = new double[TemperaturesCount];

            // extract cell voltages
            for (var i = 0; i < CellsCount; i++)
            {
                var cv = GetInt2(3 + (2 * i)) / 1000.0;
                Console.WriteLine($"Voltage #{i} {cv}");
                CellVoltages[i] = cv;
            }

            var idx = (CellsCount * 2) + 3;

            // extract cell temperatures
            for (var i = 0; i < TemperaturesCount; i++)
            {
                double tv = GetInt2Complement(idx + 1 + (2 * i));
                tv = (tv - 2731) / 10.0;

                Console.WriteLine($"Temperature #{i} {tv}");
                Temperatures[i] = tv;
            }
        }

        public override string ToString()
        {
            return $@"{base.ToString()}
CellsCount: {CellsCount}, TemperaturesCount: {TemperaturesCount}
TotalCurrent: {TotalCurrent} A, TotalVoltage: {TotalVoltage} V, RemainingCapacity: {RemainingCapacity}%, Load: {TotalCurrent * TotalVoltage} W
Quantity: {Quantity}, TotalCapacity: {TotalCapacity}, Cycles: {Cycles}
CellVoltages: {string.Join(", ", CellVoltages)}
Temperatures: {string.Join(", ", Temperatures)}
";
        }

        public class PPGetSystemParameter : PylonPacket
        {
            public PPGetSystemParameter() : base()
            {
                CID1 = 0x46;
                CID2 = 0x47;
            }
        }

        public class PPSystemParameter : PylonPacket
        {
            public byte INFOFLAG => INFO[0];
            public double UnitCellVoltage => GetInt2(1) / 1000.0;
            public double UnitCellLowVoltage => GetInt2(3) / 1000.0;
            public double UnitCellUnderVoltage => GetInt2(5) / 1000.0;

            public override string ToString()
            {
                return $@"{base.ToString()}
FLAG: {INFOFLAG}, UnitCellVoltage: {UnitCellVoltage}, UnitCellLowVoltage: {UnitCellLowVoltage}, UnitCellUnderVoltage: {UnitCellUnderVoltage}
";
            }
        }
        public class PPGetAlarmInformation : PylonPacket
        {
            public PPGetAlarmInformation() : base()
            {
                CID1 = 0x46;
                CID2 = 0x44;
                INFO = new byte[1];
                LENGTH = 0x02;
            }

            public byte Command
            {
                get => INFO[0];
                set => INFO[0] = value;
            }

            public override string ToString()
            {
                return $"{base.ToString()}, Command: {Command}";
            }
        }

        public class PPAlarmInformation : PylonPacket
        {
        }

        public class PPGetChargeManagementInformation : PylonPacket
        {
            public PPGetChargeManagementInformation() : base()
            {
                CID1 = 0x46;
                CID2 = 0x92;
                INFO = new byte[1];
                LENGTH = 0x02;
            }

            public byte Command
            {
                get => INFO[0];
                set => INFO[0] = value;
            }

            public override string ToString()
            {
                return $"{base.ToString()}, Command: {Command}";
            }
        }

        public class PPChargeManagementInformation : PylonPacket
        {
            public double VoltageUpLimit => GetInt2(1) / 1000.0;
            public double VoltageDownLimit => GetInt2(3) / 1000.0;
            public double MaxChargeCurrent => GetInt2Complement(5) / 1.0;
            public double MaxDischargeCurrent => GetInt2Complement(7) / 1.0;
            public byte Status => INFO[9];

            public override string ToString()
            {
                return $@"{base.ToString()}
VoltageUpLimit: {VoltageUpLimit}, VoltageDownLimit: {VoltageDownLimit}, MaxChargeCurrent: {MaxChargeCurrent}, MaxDischargeCurrent: {MaxDischargeCurrent}, Status: {Status}
";
            }
        }

        public class PPGetSeriesNumber : PylonPacket
        {

            public PPGetSeriesNumber() : base()
            {
                CID1 = 0x46;
                CID2 = 0x93;
                INFO = new byte[1];
                LENGTH = 0x02;
            }
            public byte Command
            {
                get => INFO[0];
                set => INFO[0] = value;
            }

            public override string ToString()
            {
                return $"{base.ToString()}, Command: {Command}";
            }
        }

        public class PPSeriesNumber : PylonPacket
        {

            public string SeriesNumber => Encoding.UTF8.GetString(INFO[1..]);

            public override string ToString()
            {
                return $@"{base.ToString()}
Series Number: {SeriesNumber}
";
            }
        }

        public class PPTurnOff : PylonPacket
        {
            public PPTurnOff() : base()
            {
                CID1 = 0x46;
                CID2 = 0x95;
                INFO = new byte[1];
                LENGTH = 0x02;
            }

            public byte Command
            {
                get => INFO[0];
                set => INFO[0] = value;
            }

            public override string ToString()
            {
                return $"{base.ToString()}, Command: {Command}";
            }
        }

        public class PPTurnOffReply : PylonPacket
        {
        }
    }
}
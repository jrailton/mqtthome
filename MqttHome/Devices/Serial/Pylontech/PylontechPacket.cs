using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MqttHome.Devices.Serial.Pylontech
{
    public class PylonPacket
    {
        public byte[] header;
        public byte[] checksum;

        public PylonPacket()
        {
            header = new byte[6];
            checksum = new byte[2];
            VER = 0x20;
            ADR = 0x02;
        }

        public byte VER
        {
            get => header[0];
            set => header[0] = value;
        }

        public byte ADR
        {
            get => header[1];
            set => header[1] = value;
        }

        public byte CID1
        {
            get => header[2];
            set => header[2] = value;
        }

        public byte CID2
        {
            get => header[3];
            set => header[3] = value;
        }

        public int LENGTH
        {
            get => (((header[4] & 0x0F) << 8) | header[5]);
            set
            {
                if (value > 0xfff || value < 0)
                    throw new Exception("Invalid length");

                var sum = (value & 0x000F) + ((value >> 4) & 0x000F) + ((value >> 8) & 0x000F);
                sum = sum % 16;
                sum = ~sum;
                sum = sum + 1;
                var val = (sum << 12) + value;
                header[5] = Convert.ToByte(val & 0xff);
                header[4] = Convert.ToByte((val >> 8) & 0xff);
            }
        }

        public byte[] INFO { get; set; }

        /// <summary>
        /// 3 byte array
        /// </summary>
        public byte[] CHKSUM
        {
            get
            {
                return checksum[0..2];
            }
            set
            {
                value.CopyTo(checksum, 0);
            }
        }

        public void UpdateChecksum()
        {
            int sum = 0;
            foreach (var b in header)
                sum += b;

            foreach (var b in INFO)
                sum += b;

            sum = sum % 65536;
            sum = ~sum;
            sum = sum + 1;

            checksum[0] = Convert.ToByte((sum >> 8) & 0xff);
            checksum[1] = Convert.ToByte(sum & 0xff);
        }

        public byte[] GetAsciiBytes()
        {

            UpdateChecksum();

            byte[] ret = new byte[header.Length + INFO.Length + checksum.Length];

            header.CopyTo(ret, 0);
            INFO.CopyTo(ret, header.Length);
            checksum.CopyTo(ret, header.Length + INFO.Length);

            var rh = '~' + string.Join("", ret.Select(b => b.ToString("X2"))) + "\r";

            Console.WriteLine($"Encoded sentence is {rh}");
            return Encoding.UTF8.GetBytes(rh);
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public PylonPacket Parse(byte[] ascii)
        {
            if (ascii.Length == 0)
                return null;

            Console.WriteLine($"Value to decode is {ascii}");


            if (ascii[0] != 0x7E || ascii[ascii.Length - 1] != 0x0D)
                throw new Exception("Invalid packet format");

            var content = Encoding.UTF8.GetString(ascii, 1, ascii.Length - 1);
            Console.WriteLine("Content of packet: %s", content);

            var bdata = StringToByteArray(content);

            var pret = new PylonPacket();

            bdata[0..pret.header.Length].CopyTo(pret.header, 0);

            if (pret.LENGTH > 0)
            {
                pret.INFO = bdata[6..-2];
                Console.WriteLine($"Info content is {pret.INFO}");
            }
            else
            {
                pret.INFO = new byte[0];
            }

            pret.UpdateChecksum();

            if (pret.checksum[0] != bdata[bdata.Length - 2] || pret.checksum[1] != bdata[bdata.Length - 1]){
                Console.WriteLine("Invalid checksum!");
                throw new Exception("Invalid checksum");
            }

            pret.PostParse();
            return pret;
        }

        public virtual void PostParse()
        {
        }

        public int GetInt2(int idx)
        {
            // if index is negative, it indicates the index relative to the end of the array (python)
            // translate it for c#
            if (idx < 0)
                idx = INFO.Length + idx;

            var val = INFO[idx] << 8 | INFO[idx + 1];
            return val;
        }

        public int GetInt2Complement(int idx)
        {
            // if index is negative, it indicates the index relative to the end of the array (python)
            // translate it for c#
            if (idx < 0)
                idx = INFO.Length + idx;

            var val = INFO[idx] << 8 | INFO[idx + 1];
            if ((val & 0x8000) == 0x8000)
                val = val - 0x10000;
            return val;
        }

        public override string ToString()
        {
            return $"VER: 0x{VER:x2}, ADR: 0x{ADR:x2}, CID1: 0x{CID1:x2}, CID2: 0x{CID2:x2}, LENGTH: {LENGTH}, len(INFO): {INFO.Length}, CHKSUM: 0x{CHKSUM[0]:X2}{CHKSUM[1]:X2}";
        }
    }
}
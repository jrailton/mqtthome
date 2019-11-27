using log4net;
using MqttHome.Devices.Serial.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace MqttHome.Devices.Serial.Axpert
{
    public class AxpertCommand
    {
        private string _port = "/dev/ttyUSB0";
        private int _baud = 2400;
        private ILog _logger;
        private bool _commandResponseComplete = false;
        private MemoryStream _responseBuffer;
        private const int _timeoutMs = 1000;
        private SerialDevice _parent; 

        public AxpertCommand(string port, int baud, SerialDevice parent) {
            _port = port;
            _baud = baud;
            _logger = parent.Controller.DeviceLog;
            _parent = parent;
        }

        public TResponse Send<TResponse>(string cmd) {

            try
            {
                var sp = new SerialPort(_port, _baud, Parity.None, 8, StopBits.One);

                sp.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                sp.ErrorReceived += new SerialErrorReceivedEventHandler(DataErrorReceivedHandler);

                sp.Open();

                byte[] commandBytes = GetMessageBytes(cmd);

                _commandResponseComplete = false;
                _responseBuffer = new MemoryStream();

                //Flush out any existing chars
                sp.ReadExisting();

                //Send request
                sp.Write(commandBytes, 0, commandBytes.Length);

                //Wait for response (or timeout)
                var startTime = DateTime.Now;
                while (!_commandResponseComplete && ((DateTime.Now - startTime).TotalMilliseconds < _timeoutMs))
                    Thread.Sleep(20);

                sp.Close();

                var response = ReadResponse();

                return (TResponse)Activator.CreateInstance(typeof(TResponse), response);
            }
            finally {
                _responseBuffer.Dispose();
                _responseBuffer = null;
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = sender as SerialPort;

            if ((sp != null) && (!_commandResponseComplete))
            {
                //Read chars until we hit a CR character
                while (sp.BytesToRead > 0)
                {
                    byte b = (byte)sp.ReadByte();
                    _responseBuffer.WriteByte(b);

                    if (b == 0x0d)
                    {
                        _commandResponseComplete = true;
                        break;
                    }
                }
            }
        }

        private void DataErrorReceivedHandler(object sender, SerialErrorReceivedEventArgs e)
        {
            _logger.Error($"DataErrorReceivedHandler :: Device: {_parent.Id} - {e.EventType}");
        }

        /// <summary>
        /// Returns response without leading ) character and trailing CRC and <cr> characters
        /// </summary>
        public string ReadResponse() {
            // payload bytes excludes <cr> character and CRC value
            byte[] payloadBytes = new byte[_responseBuffer.Length - 3];
            Array.Copy(_responseBuffer.GetBuffer(), payloadBytes, payloadBytes.Length);

            ushort crcMsb = _responseBuffer.GetBuffer()[_responseBuffer.Length - 3];
            ushort crcLsb = _responseBuffer.GetBuffer()[_responseBuffer.Length - 2];

            ushort calculatedCrc = CalculateCrc(payloadBytes);
            ushort receivedCrc = (ushort)((crcMsb << 8) | crcLsb);

            if (calculatedCrc != receivedCrc)
                throw new Exception("Response contains invalid CRC");

            var response = Encoding.ASCII.GetString(payloadBytes);

            if (!response.Substring(0, 1).Equals("("))
                throw new Exception("Response doesnt start with ( character");

            // return response without leading ) character and trailing CRC and <cr> characters
            return response.Substring(1);
        }

        /// <summary>
        /// Appends crc and CR bytes to a byte array
        /// </summary>
        private byte[] GetMessageBytes(string text)
        {
            //Get bytes for command
            byte[] command = Encoding.ASCII.GetBytes(text);

            //Get CRC for command bytes
            ushort crc = CalculateCrc(command);

            //Append CRC and CR to command
            byte[] result = new byte[command.Length + 3];
            command.CopyTo(result, 0);
            result[result.Length - 3] = (byte)((crc >> 8) & 0xFF);
            result[result.Length - 2] = (byte)((crc >> 0) & 0xFF);
            result[result.Length - 1] = 0x0d;

            return result;
        }

        /// <summary>
        /// Calculates CRC for axpert inverter
        /// Ported from crc.c: http://forums.aeva.asn.au/forums/pip4048ms-inverter_topic4332_page2.html
        /// </summary>
        private ushort CalculateCrc(byte[] pin)
        {
            ushort crc;
            byte da;
            byte ptr;
            byte bCRCHign;
            byte bCRCLow;

            int len = pin.Length;

            ushort[] crc_ta = new ushort[]
                {
                    0x0000,0x1021,0x2042,0x3063,0x4084,0x50a5,0x60c6,0x70e7,
                    0x8108,0x9129,0xa14a,0xb16b,0xc18c,0xd1ad,0xe1ce,0xf1ef
                };

            crc = 0;
            for (int index = 0; index < len; index++)
            {
                ptr = pin[index];

                da = (byte)(((byte)(crc >> 8)) >> 4);
                crc <<= 4;
                crc ^= crc_ta[da ^ (ptr >> 4)];
                da = (byte)(((byte)(crc >> 8)) >> 4);
                crc <<= 4;
                crc ^= crc_ta[da ^ (ptr & 0x0f)];
            }

            //Escape CR,LF,'H' characters
            bCRCLow = (byte)(crc & 0x00FF);
            bCRCHign = (byte)(crc >> 8);
            if (bCRCLow == 0x28 || bCRCLow == 0x0d || bCRCLow == 0x0a)
            {
                bCRCLow++;
            }
            if (bCRCHign == 0x28 || bCRCHign == 0x0d || bCRCHign == 0x0a)
            {
                bCRCHign++;
            }
            crc = (ushort)(((ushort)bCRCHign) << 8);
            crc |= bCRCLow;
            return crc;
        }
    }
}

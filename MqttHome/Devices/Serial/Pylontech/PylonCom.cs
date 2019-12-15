using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace MqttHome.Devices.Serial.Pylontech
{
    public class PylonCom
    {
        public string PORT = "COM5";
        public int BAUD = 115200;
        private SerialPort sp;

        private byte[] rxBuffer;
        private bool rxComplete;
        private int rxIndex;

        public PylonCom(string port, int baud = 115200)
        {
            PORT = port;
            BAUD = baud;

            sp = new SerialPort(PORT, BAUD);

            sp.DataReceived += OnDataReceived;

            sp.ReadTimeout = 500;
            sp.WriteTimeout = 500;

            sp.Open();

        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = sp.BytesToRead;

            sp.Read(rxBuffer, rxIndex, sp.BytesToRead);

            rxIndex += bytesToRead;

            // considered complete when last byte read is a carriage return character
            rxComplete = rxBuffer[rxIndex - 1] == 0x0D;
        }

        public PylonPacket GetReply<TPylonPacket>(PylonPacket request)
        {

            var txBuffer = request.GetAsciiBytes();

            // reset receive buffer variables
            rxBuffer = new byte[4096];
            rxComplete = false;
            rxIndex = 0;

            // send request
            sp.Write(txBuffer, 0, txBuffer.Length);

            // wait until response received or timeout
            while (!rxComplete)
            {
                Thread.Sleep(5);
            }

            Console.WriteLine($"Received sentence: {Encoding.ASCII.GetString(rxBuffer)}");

            var preply = (PylonPacket)Activator.CreateInstance(typeof(PylonPacket));
            preply.Parse(rxBuffer);

            return preply;
        }

        private void Close()
        {
            sp.Close();
        }
    }
}

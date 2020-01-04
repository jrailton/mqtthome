using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MqttHome.Devices.Serial.Base;
using MqttHome.Mqtt;
using MQTTnet;

namespace MqttHome.Devices.Serial.Pylontech
{
    public class Pylontech : SerialSensorDevice<PylontechData>, IDisposable
    {
        private Timer _timer;
        private SerialPort _sp;
        private string _rxBuffer;
        private bool _rxComplete;
        private DateTime _rxTimeout;
        private DateTime _rxStart;
        private object _timerLock = new object();

        public Pylontech(MqttHomeController controller, Config.Device config) : base(controller, DeviceType.Unknown, config)
        {
            try
            {
                if (!System.IO.Ports.SerialPort.GetPortNames().Any(s => Config.Parameters[0] == s))
                    throw new Exception();

                _timer = new Timer(OnTimer, null, 0, 1000);
            }
            catch (Exception err)
            {
                controller.DeviceLog.Error($"Pylontech..ctor :: Serial port '{config.Parameters[0]}' was not found");
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _rxBuffer += _sp.ReadExisting();

            _rxComplete = _rxBuffer.Contains("$$");

            //if (_rxComplete)
            //    Controller.DeviceLog.Info($"Pylontech comm. completed in {DateTime.Now.Subtract(_rxStart).TotalMilliseconds}ms");
        }

        public override void Dispose()
        {
            _timer.Dispose();

            if (_sp != null && _sp.IsOpen)
            {
                _sp.Close();
                _sp = null;
            }
        }

        private void OnTimer(object state)
        {
            var hasLock = false;

            try
            {
                Monitor.TryEnter(_timerLock, ref hasLock);
                if (!hasLock)
                    return;

                try
                {
                    if (_sp == null || !_sp.IsOpen)
                        InitSerialPort();

                    _rxComplete = false;
                    _rxBuffer = string.Empty;
                    _rxTimeout = DateTime.Now.AddMilliseconds(500);
                    _rxStart = DateTime.Now;
                    _sp.DiscardInBuffer();
                    _sp.WriteLine("pwrsys");

                    var timeout = false;

                    while (!_rxComplete)
                    {
                        if (DateTime.Now >= _rxTimeout)
                        {
                            Controller.DeviceLog.Error($"Pylontech :: Serial comm. timeout.");
                            timeout = true;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }

                    if (!timeout)
                    {
                        LastCommunication = DateTime.Now;
                        SensorData.Update(_rxBuffer);
                    }
                }
                catch (Exception err)
                {
                    Controller.DeviceLog.Error($"Pylontech :: Serial comm. error: {err.Message}");
                }
            }
            finally
            {
                if (hasLock)
                    Monitor.Exit(_timerLock);
            }
        }

        private void InitSerialPort() {
            try
            {
                _sp = new SerialPort(Config.Parameters[0], 115200);
                _sp.DataReceived += OnDataReceived;
                _sp.ErrorReceived += OnErrorReceived;
                _sp.Open();
            }
            catch
            {
                _sp = null;
                throw;
            }
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Controller.DeviceLog.Error($"Pylontech :: Serial comm. error: {e.EventType}");
        }
    }

    public class PylontechData : SensorData
    {
        public PylontechData()
        {
        }

        public PylontechData(string data)
        {
            // Power System Information
            // ---------------------------------
            // System is idle
            // Total Num                : 3        
            // Present Num              : 3        
            // Sleep Num                : 0        
            // System Volt              : 52876    mV
            // System Curr              : 0        mA
            // System RC                : 221556   mAH
            // System FCC               : 221556   mAH
            // System SOC               : 100      %
            // System SOH               : 100      %
            // Highest voltage          : 3534     mV
            // Average voltage          : 3525     mV
            // Lowest voltage           : 3509     mV
            // Highest temperature      : 26000    mC
            // Average temperature      : 25666    mC
            // Lowest temperature       : 25000    mC
            // Recommend chg voltage    : 53250    mV
            // Recommend dsg voltage    : 47000    mV
            // Recommend chg current    : 22200    mA
            // Recommend dsg current    : -111000  mA
            //Command completed successfully
            //$$

            //Batteries = int.Parse(Regex.Match(data, @"Total Num\s*:\s*(\d+)\s*\n").Groups[0].Value);
            Batteries = GetField(data, "Present Num");
            SystemVoltage = GetField(data, "System Volt")/1000f;
            SystemCurrent = GetField(data, "System Curr") / 1000f;
            SystemRC = GetField(data, "System RC") / 1000f;
            SystemFCC = GetField(data, "System FCC") / 1000f;
            SystemSOC = GetField(data, "System SOC");
            SystemSOH = GetField(data, "System SOH");
            HighestVoltage = GetField(data, "Highest voltage") / 1000f;
            LowestVoltage = GetField(data, "Lowest voltage") / 1000f;
            AvgVoltage = GetField(data, "Average voltage") / 1000f;
            HighestTemp = GetField(data, "Highest temperature") / 1000f;
            AvgTemp = GetField(data, "Average temperature") / 1000f;
            LowestTemp = GetField(data, "Lowest temperature") / 1000f;
        }

        public int Batteries { get; set; }
        public float SystemVoltage { get; set; }
        public float SystemCurrent { get; set; }
        public float SystemRC { get; set; }
        public float SystemFCC { get; set; }
        public int SystemSOH { get; set; }
        public int SystemSOC { get; set; }
        public float HighestVoltage { get; set; }
        public float LowestVoltage { get; set; }
        public float AvgVoltage { get; set; }
        public float HighestTemp { get; set; }
        public float LowestTemp { get; set; }
        public float AvgTemp { get; set; }
        public float SystemWatts => SystemVoltage * SystemCurrent;

        public override Dictionary<string, object> Update(string data)
        {
            return UpdateValues(new PylontechData(data));
        }

        private int GetField(string data, string name)
        {
            try
            {
                return int.Parse(Regex.Match(data, $@"{name}\s*:\s*([-\d]+)").Groups[1].Value);
            }
            catch
            {
                return 0;
            }
        }

        public override Dictionary<string, object> Update(MqttApplicationMessage message)
        {
            throw new NotImplementedException();
        }
    }
}

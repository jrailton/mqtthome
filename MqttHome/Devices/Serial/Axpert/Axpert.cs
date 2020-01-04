using log4net;
using MqttHome.Devices.Base;
using MqttHome.Devices.Serial.Base;
using MqttHome.Mqtt.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttHome.Devices.Serial.Axpert
{
    public class Axpert : SerialSensorDevice<ICCSensorData>, IDisposable
    {
        //private Timer _statusTimer;
        //private object _statusTimerLocker = new object();
        private AxpertCommand _commander;
        private ILog _logger;
        public bool _debug;
        private string _logIdentity;
        private CancellationTokenSource _cts;
        private Task _statusUpdater;

        public Axpert(MqttHomeController controller, Config.Device config) : base(controller, DeviceType.AxpertInverter, config)
        {
            try
            {
                _cts = new CancellationTokenSource();
                _logIdentity = $"Axpert (Device ID: {Id})";
                _logger = controller.DeviceLog;

                var port = config.Parameters[0];
                int baud = 2400;

                // set baud if specified -- default is 2400
                if (config.Parameters.Length > 1)
                    baud = int.Parse(config.Parameters[1]);

                if (config.Parameters.Length > 3)
                    _debug = bool.Parse(config.Parameters[3]);

                _commander = new AxpertCommand(port, baud, this, _debug);

                int statusInterval = 1000;
                if (config.Parameters.Length > 2)
                    statusInterval = int.Parse(config.Parameters[2]);

                // status timer will run every 1 second
                //_statusTimer = new Timer(StatusTimer, null, 0, statusInterval);

                Task.Run(() => StatusUpdater());

                _logger.Debug($"{_logIdentity} :: Constructor :: Started on port {port}, baud {baud}, interval {statusInterval}ms, debug {_debug}");
            }
            catch (Exception err)
            {
                controller.DeviceLog.Error($"{_logIdentity} :: Constructor :: Error - {err.Message}", err);
                throw;
            }
        }

        private void StatusUpdater()
        {
            DateTime? lastCommand = null;

            while (!_cts.Token.IsCancellationRequested)
            {
                // prevent status requests being sent more than once per second (they generally take between 400ms and 800ms on the Synapse 5.0)
                if (lastCommand.HasValue) {
                    int waitMs = 1000 - (int)DateTime.Now.Subtract(lastCommand.Value).TotalMilliseconds;
                    if (waitMs > 0)
                        Thread.Sleep(waitMs);
                }

                lastCommand = DateTime.Now;
                var response = _commander.Send("QPIGS");
                UpdateSensorData(response);
            }
        }

        // This method is called by the timer delegate. It uses thread Monitoring to prevent concurrent calls from executing or 
        // from queuing up if the method takes more than the timer interval to complete
        //private void StatusTimer(object stateInfo)
        //{
        //    var hasLock = false;

        //    try
        //    {
        //        Monitor.TryEnter(_statusTimerLocker, ref hasLock);
        //        if (!hasLock)
        //        {
        //            if (_debug)
        //                _logger.Warn($"{_logIdentity} :: StatusTimer :: Locked.");
        //            return;
        //        }

        //        // send status inquiry request
        //        var response = _commander.Send("QPIGS");

        //        UpdateSensorData(response);
        //    }
        //    finally
        //    {
        //        if (hasLock)
        //            Monitor.Exit(_statusTimerLocker);
        //    }
        //}

        public override void Dispose()
        {
            _cts.Cancel();

            // wait for status updater to stop
            while (_statusUpdater.Status == TaskStatus.Running)
                Thread.Sleep(100);

            _commander.Dispose();
        }
    }
}

using MqttHome.Devices.Base;
using MqttHome.Devices.Serial.Base;
using MqttHome.Mqtt.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace MqttHome.Devices.Serial.Axpert
{
    public class Axpert : SerialSensorDevice<ICCSensorData>
    {
        private Timer _statusTimer;
        private object _statusTimerLocker = new object();
        private AxpertCommand _commander;

        public Axpert(MqttHomeController controller, string id, string friendlyName, DeviceType type, params string[] config) : base(controller, id, friendlyName, type, config)
        {
            try
            {
                var port = config[0];
                int baud = 2400;

                // set baud if specified -- default is 2400
                if (config.Length > 1)
                    baud = int.Parse(config[1]);

                _commander = new AxpertCommand(port, baud, this);

                int statusInterval = 1000;
                if (config.Length > 2)
                    statusInterval = int.Parse(config[2]);

                // status timer will run every 1 second
                _statusTimer = new Timer(StatusTimer, null, 0, statusInterval);
            }
            catch (Exception err) {
                controller.DeviceLog.Error($"Axpert..ctor :: Error - {err.Message}");
                throw;
            }
        }

        // This method is called by the timer delegate. It uses thread Monitoring to prevent concurrent calls from executing or 
        // from queuing up if the method takes more than the timer interval to complete
        private void StatusTimer(object stateInfo)
        {
            var hasLock = false;

            try
            {
                Monitor.TryEnter(_statusTimerLocker, ref hasLock);
                if (!hasLock)
                    return;

                // send status inquiry request
                var response = _commander.Send<QpigsResponse>("QPIGS");

                UpdateSensorData(response);
            }
            finally
            {
                if (hasLock)
                    Monitor.Exit(_statusTimerLocker);
            }
        }

        private void UpdateSensorData(QpigsResponse e) {
            //var updated = SensorData.Update(e);


            //if (Controller.SaveAllSensorValuesToDatabaseEveryTime)
            //{
            //    SensorDataChanged?.Invoke(this, new SensorDataChangedEventArgs
            //    {
            //        ChangedValues = SensorData.ToDictionary()
            //    });
            //}
            //else if ((updated?.Count ?? 0) > 0 && SensorDataChanged != null)
            //{
            //    SensorDataChanged?.Invoke(this, new SensorDataChangedEventArgs
            //    {
            //        ChangedValues = updated
            //    });
            //}
        }
    }
}

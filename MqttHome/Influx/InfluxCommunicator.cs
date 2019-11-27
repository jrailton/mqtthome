using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using log4net;
using Newtonsoft.Json;

namespace MqttHome.Influx
{
    public class InfluxCommunicator
    {
        private LineProtocolClient _client;
        private ILog _logger;

        // write behind caching -- to reduce disk activity
        private LineProtocolPayload _pendingPayload;
        private Timer _writeBehindTimer;
        private const int _writeBehindInterval = 5000; // 5 seconds default

        public InfluxCommunicator(ILog logger, string influxUrl, string databaseName, string username = null, string password = null)
        {
            _logger = logger;

            try
            {
                var uri = new Uri(influxUrl);

                _logger.Debug($"InfluxCommunicator..ctor :: Connecting to Influx on '{uri.OriginalString}' using database '{databaseName}'...");

                _client = new LineProtocolClient(uri, databaseName, username, password);

                _writeBehindTimer = new Timer(OnWriteBehindTimer, null, _writeBehindInterval, _writeBehindInterval);
            }
            catch (Exception err) {
                _logger.Error($"InfluxCommunicator..ctor :: Failed to start - {err.Message}", err);
                throw new Exception("Failed to start InfluxCommunicator - see InfluxLog for details");
            }
        }

        private void OnWriteBehindTimer(object o) {
            if (_pendingPayload != null)
            {
                Write(_pendingPayload);
                _pendingPayload = null;
            }
        }

        private void Write(LineProtocolPayload payload)
        {
            try
            {
                var result = _client.WriteAsync(payload).Result;
                if (!result.Success)
                    throw new Exception($"Failed to write to InfluxDB: {result.ErrorMessage}");
            }
            catch (Exception err) {
                _logger.Error($"Write :: Failed to write to InfluxDB: {err.Message}");
            }
        }

        public void Write(LineProtocolPoint point)
        {
            // uses write behind caching -- will only write to disk maximum every 5 seconds
            if (_pendingPayload == null)
                _pendingPayload = new LineProtocolPayload();

            _pendingPayload.Add(point);
        }
    }
}

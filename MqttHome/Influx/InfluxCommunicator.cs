using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public InfluxCommunicator(ILog logger, string influxUrl, string databaseName, string username = null, string password = null)
        {
            _logger = logger;

            try
            {
                var uri = new Uri(influxUrl);

                _logger.Debug($"InfluxCommunicator.ctor :: Connecting to Influx on '{uri.OriginalString}' using database '{databaseName}'...");

                _client = new LineProtocolClient(uri, databaseName, username, password);
            }
            catch (Exception err) {
                _logger.Error($"InfluxCommunicator.ctor :: Failed to start - {err.Message}", err);
                throw new Exception("Failed to start InfluxCommunicator - see InfluxLog for details");
            }
        }

        public async void Write(LineProtocolPayload payload)
        {
            try
            {
                var result = await _client.WriteAsync(payload);
                if (!result.Success)
                    throw new Exception($"Failed to write to InfluxDB: {result.ErrorMessage}");
            }
            catch (Exception err) {
                _logger.Error($"Write :: Failed to write to InfluxDB: {err.Message}");
            }
        }

        public void Write(LineProtocolPoint point)
        {
            try
            {
                var payload = new LineProtocolPayload();

                payload.Add(point);

                Write(payload);
            }
            catch (Exception err) {
                _logger.Error($"Write :: Failed to write to InfluxDB: {err.Message}");
            }
        }
    }
}

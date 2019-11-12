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

        public InfluxCommunicator(ILog logger, Uri uri, string databaseName, string username = null, string password = null)
        {
            _logger = logger;

            _logger.Debug($"Connecting to Influx on '{uri.OriginalString}' using database '{databaseName}'...");

            _client = new LineProtocolClient(uri, databaseName, username, password);
        }

        public async void Write(LineProtocolPayload payload)
        {
            var result = await _client.WriteAsync(payload);
            if (!result.Success)
            {
                _logger.Error($"Failed to write to InfluxDB: {result.ErrorMessage}");
                throw new Exception($"Failed to write to InfluxDB: {result.ErrorMessage}");
            }
        }

        public void Write(LineProtocolPoint point)
        {
            var payload = new LineProtocolPayload();

            payload.Add(point);
            
            Write(payload);
        }
    }
}

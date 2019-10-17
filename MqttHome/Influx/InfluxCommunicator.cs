using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDbLoader.Influx
{
    public class InfluxCommunicator
    {
        private LineProtocolClient _client;

        public InfluxCommunicator(Uri uri, string databaseName, string username = null, string password = null)
        {
            _client = new LineProtocolClient(uri, databaseName, username, password);
        }

        public async void Write(LineProtocolPayload payload)
        {
            var result = await _client.WriteAsync(payload);
            if (!result.Success)
                throw new Exception($"Failed to write to InfluxDB: {result.ErrorMessage}");
        }

        public void Write(LineProtocolPoint point)
        {
            var payload = new LineProtocolPayload();

            payload.Add(point);
            
            Write(payload);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;
using Newtonsoft.Json;

namespace MqttHome.Mqtt
{
    public class SonoffPowR2SensorData : SensorData
    {
        //tele/powr2_1/STATE {"Time":"2019-10-15T12:25:55","Uptime":"0T00:15:45","Heap":14,"SleepMode":"Dynamic","Sleep":50,"LoadAvg":19,"POWER":"ON","Wifi":{"AP":1,"SSId":"rodznet","BSSId":"64:D1:54:85:38:E0","Channel":7,"RSSI":84,"LinkCount":7,"Downtime":"0T00:01:07"}}
        //tele/powr2_1/SENSOR {"Time":"2019-10-15T12:25:55","ENERGY":{"TotalStartTime":"2019-10-05T15:45:37","Total":6.592,"Yesterday":1.117,"Today":0.664,"Period":0,"Power":12,"ApparentPower":53,"ReactivePower":52,"Factor":0.22,"Voltage":229,"Current":0.232}}

        public SonoffPowR2SensorData() { }

        public SonoffPowR2SensorData(PowR2EnergyData data)
        {
            TotalStartTime = data.TotalStartTime;
            Total = data.Today;
            Yesterday = data.Yesterday;
            Today = data.Today;
            Period = data.Period;
            Power = data.Power;
            ApparentPower = data.ApparentPower;
            ReactivePower = data.ReactivePower;
            Factor = data.Factor;
            Voltage = data.Voltage;
            Current = data.Current;
    }

        public override Dictionary<string, object> Update(MqttApplicationMessage message)
        {
            return UpdateValues(new SonoffPowR2SensorData(JsonConvert.DeserializeObject<PowR2SensorData>(Encoding.UTF8.GetString(message.Payload)).ENERGY));
        }

        public DateTime TotalStartTime { get; set; }
        public float Total { get; set; }
        public float Yesterday { get; set; }
        public float Today { get; set; }
        public int Period { get; set; }
        public int Power { get; set; }
        public int ApparentPower { get; set; }
        public int ReactivePower { get; set; }
        public float Factor { get; set; }
        public int Voltage { get; set; }
        public float Current { get; set; }

        public class PowR2SensorData
        {
            public string Time { get; set; }
            public PowR2EnergyData ENERGY { get; set; }
        }

        public class PowR2EnergyData
        {
            public DateTime TotalStartTime { get; set; }
            public float Total { get; set; }
            public float Yesterday { get; set; }
            public float Today { get; set; }
            public int Period { get; set; }
            public int Power { get; set; }
            public int ApparentPower { get; set; }
            public int ReactivePower { get; set; }
            public float Factor { get; set; }
            public int Voltage { get; set; }
            public float Current { get; set; }
        }

    }
}

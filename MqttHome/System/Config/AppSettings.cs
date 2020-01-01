using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MqttHome.Config
{
    public class AppSettings
    {
        [Display(Name ="MQTT Brokers")]
        public List<MqttBroker> MqttBrokers { get; set; }

        /// <summary>
        /// Default is "http://localhost:8086"
        /// </summary>
        [Display(Name ="Influx DB URL")]
        public string InfluxDbUrl { get; set; } = "http://localhost:8086";

        /// <summary>
        /// Default is "home_db"
        /// </summary>
        [Display(Name ="Influx DB Database")]
        public string InfluxDbDatabase { get; set; } = "home_db";

        [Display(Name ="Enable Influx DB")]
        public bool InfluxDbEnabled { get; set; } = true;

        /// <summary>
        /// if false, will only save updated values to influx -- this has the advantage of less writes/data but Grafana queries will need to cater for "missing" values 
        /// i.e. use "previous" which doesnt actually work if "previous" is outside of selected date range
        /// </summary>
        [Display(Name ="Save All Sensor Values To Database Every Time")]
        public bool SaveAllSensorValuesToDatabaseEveryTime { get; set; } = true;

        [Display(Name = "Longitude", Description = "Used to calculate sunrise/sunset times")]
        public double Longitude { get; set; }

        [Display(Name ="Latitude", Description = "Used to calculate sunrise/sunset times")]
        public double Latitude { get; set; }

        [Display(Name ="Enable Rule Engine")]
        public bool RuleEngineEnabled { get; set; } = false;

        [Display(Name ="Google Application ID", Description ="For OAuth and Nest Integration")]
        public string GoogleAppId { get; set; }

        [Display(Name ="Google Application Secret Key", Description = "For OAuth and Nest Integration")]
        public string GoogleAppSecret { get; set; }

        [Display(Name ="Administrator Users")]
        public List<string> AdminUsers { get; set; }

        [Display(Name ="Operator Users")]
        public List<string> OperatorUsers { get; set; }

        [Display(Name ="Read Only (Viewer) Users")]
        public List<string> ViewerUsers { get; set; }

        [Display(Name ="Application Base URL")]
        public string BaseUrl { get; set; }

        [Display(Name ="Push Messaging Public Key")]
        public string PushPublicKey { get; set; }

        [Display(Name ="Push Messaging Private Key")]
        public string PushPrivateKey { get; set; }

        [Display(Name ="Debug Mode")]
        public bool Debug { get; set; } = false;
    }

    public class MqttBroker
    {
        public string IpAddress { get; set; }
        public string Name { get; set; }
        public int Port { get; set; } = 1883;
    }

}

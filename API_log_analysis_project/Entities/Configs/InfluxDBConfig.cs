using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Entities.Configs
{
    public class InfluxDBConfig
    {
        private static InfluxDBConfig? config { get; set; }
        public string InfluxUrl { get; set; }
        public string Token { get; set; }
        public string Org { get; set; }
        public string OrgId { get; set; }
        public string Bucket { get; set; }
        public bool IsEnabled { get; set; }

        private InfluxDBConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            InfluxUrl = config["InfluxDBConfig:InfluxUrl"];
            Token = config["InfluxDBConfig:InfluxToken"];
            Org = config["InfluxDBConfig:Org"];
            OrgId = config["InfluxDBConfig:OrgId"];
            Bucket = config["InfluxDBConfig:Bucket"];
            IsEnabled = bool.Parse(config["InfluxDBConfig:IsEnabled"]);

        }

        public static InfluxDBConfig GetInstance()
        {
            if (config == null)
            {
                config = new InfluxDBConfig();
            }
            return config;
        }
    }
}

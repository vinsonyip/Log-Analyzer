using API_log_analysis_project.Common.UI;
using API_log_analysis_project.Common;
using API_log_analysis_project.Entities;
using API_log_analysis_project.Factories;
using API_log_analysis_project.Filters;
using API_log_analysis_project.Groupers;
using API_log_analysis_project.Trackers;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.DBWriters
{
    interface IDBWriter
    {
        public enum ExportDest { InfluxDB, Txt }

        public Task CheckAndCreateBucketIfNotExist();
        public Task<LogProfile> WriteLogToInfluxDBAsync(LogProfile logProfile, ILogParsingTracker logParsingTracker);
        public Task<LogProfile> WriteLogToTxtAsync(LogProfile logProfile, ILogParsingTracker logParsingTracker);
    }
}

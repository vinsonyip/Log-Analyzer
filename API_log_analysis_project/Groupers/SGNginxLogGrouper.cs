using API_log_analysis_project.Common;
using API_log_analysis_project.Entities;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Groupers
{
    public class SGNginxLogGrouper : LogGrouper
    {
        protected override void PopPreviousLogRecordToFlushList()
        {
            List<string> keysToBeDeleted = new List<string>();

            foreach (var key in logGroups.Keys)
            {
                keysToBeDeleted.Add(key);
                LogDataPoint logDP = logGroups[key];
                if (logDP.Timestamp != null)
                {
                    var point = PointData.Measurement(GlobalStore.InfluxDB_SGNGINXLogMeasureName)
                        //.Tag("level", logDP.Level)
                        //.Tag("accode", logDP.Accode)
                        .Tag("status_code", logDP.StatusCode)
                        //.Tag("base_url", logDP.BaseUrl)
                        .Tag("action", logDP.Action)
                        //.Field("http_method", logDP.Method)
                        .Field("duration_ms", logDP.DurationMs)
                        //.Field("url", logDP.Url)
                        .Field("parameters", logDP.Parameters)
                        .Timestamp((DateTime)logDP.Timestamp, WritePrecision.Ms);

                    flushList.Add(point);
                    flushListSource.Add(logDP);
                }
            }

            foreach (var key in keysToBeDeleted)
            {
                logGroups.Remove(key);
            }
        }
    }
}

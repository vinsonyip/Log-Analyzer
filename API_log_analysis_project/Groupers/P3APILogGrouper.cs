using API_log_analysis_project.Common;
using API_log_analysis_project.Entities;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace API_log_analysis_project.Groupers
{
    /// <summary>
    /// How to group the log records?
    /// - Depends on [Timestamp],[task ID],[Action]
    /// - Conditions identify the duplicated record
    ///     * Condition 1: If more than 1 record with the same [Timestamp], [taskID] and [Action], then it claims to be the same
    /// 
    /// Logic:
    /// - 1. Save the [Timestamp], [taskID] and [Action] as [Timestamp]_[taskID]_[Action] as key to the Dictionary, and LogDataPoint object as value
    /// - 2. Get the next line of log record to form the key
    /// - 3. If the next record don't have the same key as the previous record, then flush the record of previous key from Dictionary to the InfluxDB
    /// </summary>
    public class P3APILogGrouper : LogGrouper
    {
        protected override void PopPreviousLogRecordToFlushList()
        {
            List<string> keysToBeDeleted = new List<string>();

            foreach (var key in logGroups.Keys)
            {
                keysToBeDeleted.Add(key);
                LogDataPoint logDP = logGroups[key];
                
                if(logDP.Timestamp != null)
                {
                    var point = PointData.Measurement(GlobalStore.InfluxDB_P3APILogMeasureName)
                    .Tag("level", logDP.Level)
                    .Tag("accode", logDP.Accode)
                    .Tag("status_code", logDP.StatusCode)
                    .Tag("base_url", logDP.BaseUrl)
                    .Tag("action", logDP.Action)
                    //.Field("http_method", logDP.Method)
                    //.Field("duration_ms", logDP.DurationMs)
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

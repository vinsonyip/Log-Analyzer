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
    public class P3SMSAPILogGrouper: LogGrouper
    {
        /// <summary>
        /// If new logDataPoint comes in and it doesn't match any of the existing key, then push the previous record to flush list
        /// Use [Timestamp]_[TaskId]_[Action] to detect and group the duplicated record
        /// </summary>
        /// <param name="logDataPoint"></param>
        public override void GroupDuplicateLog(LogDataPoint? logDataPoint)
        {
            if (logDataPoint != null)
            {
                string logGroupingKey = $"{logDataPoint.Timestamp.ToString()}_{logDataPoint.TaskId}_{logDataPoint.Action}";
                if (logGroups.ContainsKey(logGroupingKey))
                {
                    var logEntry = logGroups[logGroupingKey];
                    // Todo: Append attributes to the LogDataPoint object if duplicated record detected, to enrich the attribute
                    if (logDataPoint.Level.Length > 0) logEntry.Level = logDataPoint.Level;
                    if (logDataPoint.Protocol.Length > 0) logEntry.Protocol = logDataPoint.Protocol;
                    if (logDataPoint.Method.Length > 0) logEntry.Method = logDataPoint.Method;
                    if (logDataPoint.Url.Length > 0) logEntry.Url = logDataPoint.Url;
                    if (logDataPoint.BaseUrl.Length > 0) logEntry.BaseUrl = logDataPoint.BaseUrl;
                    if (logDataPoint.ContentType.Length > 0) logEntry.ContentType = logDataPoint.ContentType;
                    if (logDataPoint.ResponseContentType.Length > 0) logEntry.ResponseContentType = logDataPoint.ResponseContentType;
                    if (logDataPoint.Accode.Length > 0) logEntry.Accode = logDataPoint.Accode;
                    if (logDataPoint.Parameters.Length > 0) logEntry.Parameters = logDataPoint.Parameters;
                    if (logDataPoint.ContentLength > 0) logEntry.ContentLength = logDataPoint.ContentLength;
                    if (logDataPoint.DurationMs > 0) logEntry.DurationMs = logDataPoint.DurationMs;
                    if (logDataPoint.IsRequest != null) logEntry.IsRequest = logDataPoint.IsRequest;
                    logEntry.GroupItemCumCount += 1;
                }
                else
                {
                    PopPreviousLogRecordToFlushList();
                    logGroups.Add(logGroupingKey, logDataPoint);
                }
            }
            else
            {
                // Even if the log data point is null, we still need to count the offset of line read
                string key = logGroups.Keys.FirstOrDefault();
                // We choose one of the log group to put the null record inside, just to count the offset
                if (key != null)
                {
                    logGroups[key].GroupItemCumCount += 1;
                }
            }

        }
        protected override void PopPreviousLogRecordToFlushList()
        {
            List<string> keysToBeDeleted = new List<string>();

            foreach (var key in logGroups.Keys)
            {
                keysToBeDeleted.Add(key);
                LogDataPoint logDP = logGroups[key];
                var point = PointData.Measurement(GlobalStore.InfluxDB_P3SMSAPILogMeasureName)
                    .Tag("level", logDP.Level)
                    .Tag("accode", logDP.Accode)
                    .Tag("status_code", logDP.StatusCode)
                    .Tag("base_url", logDP.BaseUrl)
                    .Tag("action", logDP.Action)
                    .Field("http_method", logDP.Method)
                    .Field("duration_ms", logDP.DurationMs)
                //.Field("url", logDP.Url)
                    .Field("parameters", logDP.Parameters)
                    .Timestamp((DateTime)logDP.Timestamp, WritePrecision.Ms);

                flushList.Add(point);
                flushListSource.Add(logDP);
            }

            foreach (var key in keysToBeDeleted)
            {
                logGroups.Remove(key);
            }
        }
    }
}

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
    public class LogGrouper : ILogGrouper
    {
        protected Dictionary<string, LogDataPoint> logGroups = new();
        protected List<PointData> flushList { get; set; } = new(); // Log data point to be flushed to Influx DB next round.
        protected List<LogDataPoint> flushListSource { get; set; } = new();

        public LogGrouperResult? Execute(LogDataPoint logDataPoint, int batchSize = 1)
        {
            GroupDuplicateLog(logDataPoint);
            return GetLogGrouperResultByBatchSize(batchSize);
        }
        public virtual void GroupDuplicateLog(LogDataPoint? logDataPoint)
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
                else
                {
                    PopPreviousLogRecordToFlushList();
                    logGroups.Add("Unknown", new LogDataPoint());
                }
            }

        }
        protected virtual void PopPreviousLogRecordToFlushList()
        {
            List<string> keysToBeDeleted = new List<string>();

            foreach (var key in logGroups.Keys)
            {
                keysToBeDeleted.Add(key);
                LogDataPoint logDP = logGroups[key];
                var point = PointData.Measurement(GlobalStore.InfluxDB_P3APILogMeasureName)
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

        public LogGrouperResult? GetRemainLogGrouperResult()
        {
            PopPreviousLogRecordToFlushList();
            return new LogGrouperResult
            {
                PointDataFlushList = GetAndClearFlushList() ?? [],
                LogDataPointFlushList = GetAndClearFlushListSource() ?? []
            };
        }

        // Each log should define their own data points, so set this to virtual


        public int GetFlushListLength()
        {
            return flushList.Count;
        }

        public List<PointData>? GetFlushList()
        {
            return flushList;
        }

        protected List<PointData>? GetAndClearFlushList()
        {
            if (flushList == null || flushList.Count <= 0) return null;
            var tmpFlushList = new List<PointData>(flushList);
            flushList.Clear();
            return tmpFlushList;
        }
        protected List<LogDataPoint>? GetAndClearFlushListSource()
        {
            if (flushListSource == null || flushListSource.Count <= 0) return null;
            var tmpFlushList = new List<LogDataPoint>(flushListSource);
            flushListSource.Clear();
            return tmpFlushList;
        }


        /// <summary>
        /// Only when flush list length hit the batch size, then it will returns the whole flush list;
        /// If not hit the batch size, then return nothing.
        /// Example: Batch size = 1, means when 1 record inside the flush list then GetFlushListByBatchSize(int batchSize) returns the whole list,else return nothing 
        /// </summary>
        /// <returns>flushList</returns>

        protected LogGrouperResult? GetLogGrouperResultByBatchSize(int batchSize)
        {
            if (flushList.Count > batchSize)
            {
                return new LogGrouperResult
                {
                    PointDataFlushList = GetAndClearFlushList() ?? [],
                    LogDataPointFlushList = GetAndClearFlushListSource() ?? []
                };
            }

            return null;
        }
    }
}

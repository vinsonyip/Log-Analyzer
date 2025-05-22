using InfluxDB.Client.Writes;

namespace API_log_analysis_project.Entities
{
    public class LogGrouperResult
    {
        /// <summary>
        /// Log data point to be flushed to Influx DB.
        /// </summary>
        public List<PointData> PointDataFlushList { get; set; } = new();

        /// <summary>
        /// LogDataPoint: A class to track the log parsing information(Meta data), including the offset of the log record relative position in raw log file
        /// </summary>
        public List<LogDataPoint> LogDataPointFlushList { get; set; } = new();
    }
}

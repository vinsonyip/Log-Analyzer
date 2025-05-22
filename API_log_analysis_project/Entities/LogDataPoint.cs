namespace API_log_analysis_project.Entities
{
    public class LogDataPoint
    {
        public DateTime? Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string ResponseContentType { get; set; } = string.Empty;
        public string Accode { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int ContentLength { get; set; } 
        public double DurationMs { get; set; } 
        public string TaskId { get; set; } = string.Empty;
        public bool? IsRequest { get; set; } // True: Log is request log, False: Log is response log, Null: log is unknown type
        public int GroupItemCumCount { get; set; } = 1; // This is used to count how much element being grouped into this data point cumulatively, by default, 1 data point should consist of 1 record => GroupItemCumCount = 1;
    }

}

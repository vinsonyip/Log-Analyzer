using API_log_analysis_project.Factories;
using API_log_analysis_project.Filters;
using API_log_analysis_project.Groupers;
using System.Text.Json.Serialization;

namespace API_log_analysis_project.Entities
{
    public class LogProfile
    {
        public string LogFilePath { get; set; } = string.Empty;
        public string LogName { get; set; } = string.Empty;
        public ParserName Parser { get; set; } = ParserName.P3_SMS_API_Parser;
        public string ParserNameStr { get; set; } = Enum.GetName(ParserName.P3_SMS_API_Parser) ?? "";
        public string Status { get; set; } = "P";
        public string Message { get; set; } = "";
        public int LastLine { get; set; } = 0;
        public double ProgressPercent { get; set; } = 0.0;
        public string OutputPath { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        [JsonIgnore]
        public ILogParser? LogParser { get; set; }
        [JsonIgnore]
        public ILogFilter? LogFilter { get; set; }
        [JsonIgnore]
        public ILogGrouper? LogGrouper { get; set; }

    }
}

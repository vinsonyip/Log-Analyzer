namespace API_log_analysis_project.Entities
{
    public class LogParsingReport
    {
        public List<LogProfile> Profiles { get; set; } = new List<LogProfile>();
        public string Message { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}

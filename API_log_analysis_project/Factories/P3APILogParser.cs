using API_log_analysis_project.Entities;
using API_log_analysis_project.Filters;
using API_log_analysis_project.Parsers;

namespace API_log_analysis_project.Factories
{
    public class P3APILogParser : ILogParser
    {
        private LogDataPoint? parse(string rawLog)
        {

            // Console.WriteLine("\n\n===== Fetch Input and Result (Request or response) =====");
            string inputOutputStr = new InputOutputParser().parse(ref rawLog);
            // Console.WriteLine("\n\n===== Fetch task id =====");
            string taskIdStr = new TaskIdParser().parse(ref rawLog);
            // Console.WriteLine("\n\n===== Fetch message level =====");
            string msgLevelStr = new MessageLevelParser().parse(ref rawLog);
            // Console.WriteLine("\n\n===== Fetch timestamp =====");
            DateTime? timestampStr = new TimestampParser().parseToDateTime(ref rawLog);
            // Console.WriteLine("\n\n===== Fetch API action =====");
            string actionStr = new APIActionParser().parse(ref rawLog);
            // Console.WriteLine("\n\n===== Fetch Accode =====");
            string accodeStr = new AccodeParser().parse(ref rawLog);
            // Console.WriteLine("\n\n===== Fetch Url =====");
            string urlStr = new UrlParser().parse(ref rawLog);
            // Console.WriteLine("\n\n===== Fetch parameter =====");
            string parameterStr = new ParameterParser().parse(ref rawLog);
            // Console.WriteLine("\n\n===== Fetch StatusCode =====");
            string statusCodeStr = new StatusCodeParser().parse(ref rawLog);
            // Console.WriteLine("\n\n===== End of process =====\n\n");

            bool? isRequest = null;

            if (inputOutputStr != null)
            {
                if (inputOutputStr == "INPUT") isRequest = true;
                else if (inputOutputStr == "RESULT") isRequest = false;
                else isRequest = null;
            }

            return new LogDataPoint()
            {
                TaskId = taskIdStr,
                Level = msgLevelStr,
                Timestamp = timestampStr,
                Action = actionStr,
                Accode = accodeStr,
                IsRequest = isRequest
            };
        }

        public LogDataPoint? parse(string rawLog, ILogFilter? logFilter = null)
        {
            var logDataPoint = parse(rawLog);
            if(logFilter == null) return logDataPoint;

            P3APILogFilter p3APILogFilter = logFilter as P3APILogFilter;

            if (p3APILogFilter.execute(logDataPoint)) return logDataPoint;

            return null;
        }
    }
}

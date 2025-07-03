using API_log_analysis_project.Entities;
using API_log_analysis_project.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API_log_analysis_project.Factories
{
    public class SGNginxLogParser : ILogParser
    {
        private static string pattern = "^(\\S+) - (\\S+) \\[(.*?)\\] " +
                "\"(.*(?:/gmobile|/ods).*?)\" (\\d{3}) (\\d+|-) " +
                "\"(.*?)\" \"(.*?)\" " +
                "-- proxy response time: ([\\d.]+|-) seconds -- " +
                "-- backend response time: ([\\d.]+|-) seconds -- " +
                "Balancer variables: \"(.*?)\" \"(.*?)\" \"(.*?)\" " +
                "-- server local IP: \"(.*?)\" (\\d{1,3}|-) -- " +
                "client IP: \"(.*?)\"$";
        public LogDataPoint? parse(string rawLog, ILogFilter? logFilter = null)
        {
            Match match = Regex.Match(rawLog, pattern);

            if (match.Success)
            {
                // Extract timestamp
                string timestamp = match.Groups[3].Value;
                string format = "dd/MMM/yyyy:HH:mm:ss zzz";
                DateTime timestampDateTime = DateTime.ParseExact(timestamp, format, CultureInfo.InvariantCulture);

                // Extract action
                string action = match.Groups[4].Value;
                var actionSplitList = action.Split(" "); 
                action = actionSplitList[1];

                // Extract request params
                List<string> urlSplit = action.Split("?").ToList();
                action = urlSplit[0];
                string requestParams = urlSplit.Count >= 2 ? urlSplit[1] : "";

                // Extract status code
                string statusCode = match.Groups[5].Value;

                // Extract response time
                string responseTime = match.Groups[10].Value;
                double responseTimeDbl = 0.0;
                if (double.TryParse(responseTime, out var result)) responseTimeDbl = result * 100;
                
                //Console.WriteLine($"Timestamp: {timestampDateTime.ToString()}");
                //Console.WriteLine($"Action: {action}");
                //Console.WriteLine($"Response time: {responseTimeDbl}");
                return new LogDataPoint
                {
                    Timestamp = timestampDateTime,
                    Action = action,
                    Parameters = requestParams,
                    StatusCode = statusCode,
                    DurationMs = responseTimeDbl
                };
            }
            else
            {
                return null;
            } 
        }
    }
}

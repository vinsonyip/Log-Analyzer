using System.Text.RegularExpressions;
using API_log_analysis_project.Entities;
using System.Globalization;
using System;
using System.IO;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using InfluxDB.Client;
using API_log_analysis_project.Filters;
using API_log_analysis_project.Parsers;

namespace API_log_analysis_project.Factories
{
    public class P3SMSAPILogParser: ILogParser
    {
        private LogDataPoint? parseNormalResponse(string rawLog)
        {
            List<string> logFieldSplit = rawLog.Trim().Split(" ").ToList();
            
            if (logFieldSplit.Count != 16) return null; // Only `Request finished` record line consist of 16 fields, so this condition filter the log naturally.

            string timestampStr = logFieldSplit[0] + " " + logFieldSplit[1] + " " + logFieldSplit[2];
            DateTime timestamp = DateTime.ParseExact(timestampStr, "yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture);

            string logLevel = logFieldSplit[3].Substring(1, 3);
            string protocol = logFieldSplit[6];
            string url = logFieldSplit[8];
            string durationMs = "";
            string accode = "";
            string requestParams = "";
            string baseUrl = "";

            List<string> urlSplit = url.Split("?").ToList();
            baseUrl = urlSplit[0];
            requestParams = urlSplit.Count >= 2 ? urlSplit[1] : "";

            // Process URL fields
            if (url.ToUpper().Contains("BOND"))
            {
                // e.g. {{base_url}}/P3API/Bond/Account/{{accode}}/GetBondPosition?ISINCode=XS2103199050
                List<string> baseUrlSplit = baseUrl.Split("/Account/").ToList();

                if (baseUrlSplit.Count >= 2)
                {
                    var accodeSplit = baseUrlSplit[1].Split("/");
                    if (accodeSplit.Length >= 1) accode = accodeSplit[0];
                }

                baseUrl = baseUrl.Replace(accode, "{{Account}}");
            }
            else
            {
                List<string> requestParamSplit = urlSplit.Count >= 2 ? urlSplit[1].Split("&").ToList() : [];

                foreach (string param in requestParamSplit)
                {
                    string upperCaseParam = param.ToUpper();
                    if (upperCaseParam.Contains("ACC"))
                    {
                        var accodeParamSplit = upperCaseParam.Split('=');
                        accode = accodeParamSplit.Count() >= 2 ? accodeParamSplit[1] : "";
                    }
                }
            }
            string action = baseUrl.Split("/P3API/P3API").Last();
            durationMs = logFieldSplit[15].Length >= 2 ? logFieldSplit[15].Substring(0, logFieldSplit[15].Length - 2) : "0";

            return new LogDataPoint
            {
                Accode = accode,
                Timestamp = timestamp,
                Level = logFieldSplit[3].Substring(1, 3),
                Protocol = logFieldSplit[6],
                Method = logFieldSplit[7],
                Url = logFieldSplit[8],
                Parameters = requestParams,
                BaseUrl = baseUrl,
                Action = action,
                ContentType = logFieldSplit[9],
                ContentLength = int.TryParse(logFieldSplit[10], out int contentLength) ? contentLength : 0,
                StatusCode = logFieldSplit[12],
                ResponseContentType = logFieldSplit[14],
                DurationMs = double.TryParse(durationMs, out double tmpDurationMs) ? tmpDurationMs : 0
            };
        }
        private LogDataPoint? parseErrResponse(string rawLog)
        {
            // Console.WriteLine("\n\n===== Fetch message level =====");
            string msgLevelStr = new MessageLevelParser().parse(ref rawLog);
            if (msgLevelStr.Length == 0 || msgLevelStr.ToUpper() == "INF")  return null;

            // Console.WriteLine("\n\n===== Fetch timestamp =====");
            string timestampStr = new TimestampParser().parse(ref rawLog);
            DateTime? timestamp = null;
            if(DateTime.TryParse(timestampStr, out DateTime result)) timestamp = result;

            // Console.WriteLine("\n\n===== Fetch API action =====");
            string actionStr = new APIActionParser().parse(ref rawLog);


            return new LogDataPoint()
            {
                Level = msgLevelStr,
                Timestamp = timestamp,
                Action = actionStr,
                //Accode = accodeStr,
            };
        }

        public LogDataPoint? parse(string rawLog, ILogFilter? logFilter = null)
        {
            // Todo: Need to impl the log filter feature here
            // The function only parse the response now, no filter applied to SMS log
            LogDataPoint? logDataPoint = parseNormalResponse(rawLog);
            if (logDataPoint == null)
            {
                logDataPoint = parseErrResponse(rawLog);
            }
            return logDataPoint;
        }
    }
}

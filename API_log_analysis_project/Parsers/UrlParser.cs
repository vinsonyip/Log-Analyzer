using System.Text.RegularExpressions;

namespace API_log_analysis_project.Parsers
{
    public class UrlParser : BaseParser
    {
        /// <summary>
        /// Pattern1: pattern: @"\b(GET|POST|PUT|DELETE)\s+(https?://[^\s\?]+)"
        ///           source: P3 API log
        ///           match example: `2025-02-28 15:51:56.207 [default task-15319] INFO: IPONotificationClient.Function name: callPostApi - Function name calling: getStockPosition => Request: POST http://10.183.100.41:8888/P3API/P3API/GetStockPosition_App?AcctNo=M570350&Lang=GB&LivePrice=1 HTTP/1.1 - Body: null - Param: {AcctNo=M570350, Lang=GB, LivePrice=1}`
        ///           extract accode result: http://10.183.100.41:8888/P3API/P3API/GetStockPosition_App
        /// 
        /// </summary>

        public enum UrlPattern { Pattern1, Pattern2, Pattern3 };
        public UrlParser() : base()
        {
            Patterns = new List<string>() { @"\b(GET|POST|PUT|DELETE)\s+(https?://[^\s\?]+)" };
        }

        /// <summary>
        /// Use a specific pattern to extract timestamp
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="apiAction"></param>
        /// <returns></returns>

        
        public override string parse(ref string logEntry)
        {
            foreach (string pattern in Patterns)
            {
                Match match = Regex.Match(logEntry, pattern);
                if (match.Success)
                {
                    // Extract the url content inside the braces
                    string url = match.Groups[2].Value;
                    // Replace the matched pattern with an empty string
                    logEntry = Regex.Replace(logEntry, pattern, "");
                    // return Url
                    return url;
                }
            }
            return "";
        }
        public string parse(ref string logEntry, UrlPattern urlPattern)
        {
            // Extract the timestamp using Regex
            if (Patterns.Count > (int)urlPattern)
            {
                string pattern = Patterns[(int)urlPattern];
                string url = parse(ref logEntry, pattern);
                if (url.Length > 0) return url;
            }

            return "";
        }

    }
}

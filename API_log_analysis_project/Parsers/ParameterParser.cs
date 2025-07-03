using System.Text.RegularExpressions;

namespace API_log_analysis_project.Parsers
{
    public class ParameterParser : BaseParser
    {
        /// <summary>
        /// Pattern1: pattern: @"Param:\s*\{([^}]*)\}"
        ///           source: P3 API log
        ///           match example: `2025-02-28 15:50:24.176 [default task-15317] INFO: IPONotificationClient.Function name: callPostApi - Function name calling: getStockPosition => Request: POST http://10.183.100.41:8888/P3API/P3API/GetStockPosition_App?AcctNo=M505417&Lang=GB&LivePrice=1 HTTP/1.1 - Body: null - Param: {AcctNo=M505417, Lang=GB, LivePrice=1}`
        ///           extract parameter result: AcctNo=M505417, Lang=GB, LivePrice=1
        /// 
        /// </summary>

        public enum ParameterPattern { Pattern1, Pattern2, Pattern3 };
        public ParameterParser() : base()
        {
            // Patterns = new List<string>() { @"(?<=(?:AcctNo|accountNo):\s*)(?:[A-Za-z]+\d+|\d+)", @"(?<=accountNo=)[^&\s]+", @"(?<=[?&](?:accountNo|AcctNo|AccountList)=)[^&\s]+" };
            Patterns = new List<string>() { @"(?i)input:\s*\[(.*?)\]", @"Param:\s*\{([^}]*)\}" };
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
                    // Extract the parameters content inside the braces
                    string paramsContent = match.Groups[1].Value;
                    // Replace the matched pattern with an empty string
                    logEntry = Regex.Replace(logEntry, pattern, "");
                    // return param
                    return paramsContent;
                }
            }
            return "";
        }
        public string parse(ref string logEntry, ParameterPattern parameterPattern)
        {
            // Extract the timestamp using Regex
            if (Patterns.Count > (int)parameterPattern)
            {
                string pattern = Patterns[(int)parameterPattern];
                string parameter = parse(ref logEntry, pattern);
                if (parameter.Length > 0) return parameter;
            }

            return "";
        }

    }
}

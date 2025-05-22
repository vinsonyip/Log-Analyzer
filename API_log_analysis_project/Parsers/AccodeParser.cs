using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Parsers
{
    public class AccodeParser : BaseParser
    {
        /// <summary>
        /// Pattern1: pattern: @"(?<=(?:AcctNo|accountNo):\s*)[A-Za-z0-9]+"
        ///           source: P3 API log
        ///           match example: `2025-02-28 15:58:44.740 [default task-15035] INFO: AuthenticationService.getDefaultAccount => AcctNo:100028252; M547889`
        ///           extract accode result: 100028252
        /// 
        /// Pattern2: pattern: @"(?<=(?:AcctNo|accountNo):)[A-Za-z]*\d+[A-Za-z]*", @"(?<=accountNo=)[^&\s]+"
        ///           source: P3 API log
        ///           match example: `2025-02-28 15:58:44.004 [default task-15051] INFO: IPONotificationClient.Function name: callPostApi - Function name calling: getLSBalance => Request: POST http://10.183.100.41:8888/P3API/P3API/GetLSBalance_App?accountNo=M540540 HTTP/1.1 - Body: null - Param: {accountNo=M540540}`
        ///           extracted accode result: M540540
        /// 
        /// Pattern3: pattern: @"(?<=[?&](?:accountNo|AcctNo|AccountList)=)[^&\s]+"
        ///           source: P3 SMS API log
        ///           match example1: `2025-03-20 21:03:26.429 +08:00 [INF] Request finished HTTP/1.1 POST http://10.183.100.41:8888/P3API/P3API/GetProductAccount?product=32&accountNo=M566670 application/json;charset=UTF-8 0 - 200 - application/json;+charset=utf-8 3.8039ms`
        ///           match example2: `2025-03-20 21:03:26.463 +08:00 [INF] Request finished HTTP/1.1 POST http://10.183.100.41:8888/P3API/P3API/GetStockPosition_App?AcctNo=M566670&Lang=GB&LivePrice=1 application/json 0 - 200 355 application/json 12.0447ms `
        ///           match example3: `2025-03-20 21:03:40.404 +08:00 [INF] Request finished HTTP/1.1 POST http://10.183.100.41:8888/P3API/P3API/SendEmail?Language=GB&EmailContentNo=1&ParameterList=%7E%3A%7E2025-03-20+21%3A03%3A40%7E%3A%7EiPhone&AccountList=M566670 application/json 0 - 200 - text/plain;+charset=utf-8 41.2837ms`
        ///           extracted accode result: M566670
        /// </summary>

        public enum AccodePattern { Pattern1, Pattern2, Pattern3 };
        public AccodeParser() : base()
        {
            Patterns = new List<string>() { @"(?<=(?:AcctNo|accountNo):\s*)(?:[A-Za-z]+\d+|\d+)", @"(?<=accountNo=)[^&\s]+", @"(?<=[?&](?:accountNo|AcctNo|AccountList)=)[^&\s]+" };
        }

        /// <summary>
        /// Use a specific pattern to extract timestamp
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="apiAction"></param>
        /// <returns></returns>
        public string parse(ref string logEntry, AccodePattern accodePattern)
        {
            // Extract the timestamp using Regex
            if (Patterns.Count > (int)accodePattern)
            {
                string pattern = Patterns[(int)accodePattern];
                string accode = parse(ref logEntry, pattern);
                if (accode.Length > 0) return accode;
            }

            return "";
        }

    }
}

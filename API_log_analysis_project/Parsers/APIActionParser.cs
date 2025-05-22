using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Parsers
{
    public class APIActionParser: BaseParser
    {
        /// <summary>
        /// Pattern1: pattern: @"\b[A-Za-z]+\.[A-Za-z]+\b(?=\s*=>)"
        ///           source: P3 API log
        ///           match example1: `2025-02-28 15:58:44.740 [default task-15035] INFO: AuthenticationService.getDefaultAccount => AcctNo:100028252; M547889`
        ///           match example2: `2025-02-28 16:15:17.632 [default task-15066] INFO: EconomicCalAPIClient.getAllEventDatesV1 => AcctNo:current UTC2025-02-262025-03-02; dateTimeLocal2025-02-26T08:00+08:00[Asia/Singapore]>>2025-02-26T08:00: zone Range:2025-02-26T00:002025-03-02T23:59:59todayDate2025-02-28T00:00`
        ///           extract action result: `AuthenticationService.getDefaultAccount`
        ///           
        /// Pattern2: pattern: @"(?<=/)[A-Za-z0-9_]+(?=\?|$)"
        ///           source: P3 API log
        ///           match example1: `2025-02-28 15:58:44.004 [default task-15051] INFO: IPONotificationClient.Function name: callPostApi - Function name calling: getLSBalance => Request: POST http://10.183.100.41:8888/P3API/P3API/GetLSBalance_App?accountNo=M540540 HTTP/1.1 - Body: null - Param: {accountNo=M540540}`
        ///           match example2: `2025-02-28 16:15:17.408 [default task-15132] INFO: IPONotificationClient.After calling: Function name calling: getP3MarginRatio => AcctNo:Response:; HttpResponseProxy{HTTP/1.1 200 OK [Transfer-Encoding: chunked, Content-Type: text/plain; charset=utf-8, Server: Microsoft-IIS/10.0, X-Powered-By: ASP.NET, Date: Fri, 28 Feb 2025 08:15:16 GMT] ResponseEntityProxy{[Content-Type: text/plain; charset=utf-8,Chunked: true]}}
        ///           extracted action result: `GetLSBalance_App`
        ///           
        /// Pattern3: pattern: @"[A-Za-z0-9_]+\.[A-Za-z0-9_]+(?=\-\>)";
        ///           source: P3 API log
        ///           match example: 2025-02-28 16:13:14.023 [default task-15156] WARN: InboxDA.getYourMessagesCount->DB => 0
        ///           extracted action result: `InboxDA.getYourMessagesCount`
        ///           
        /// Pattern4: pattern: @"(?<=/)[A-Za-z]+(?=\?|$)"
        ///           source: P3 SMS API log
        ///           match example: `2025-03-20 20:51:58.315 +08:00 [INF] Request finished HTTP/1.1 POST http://10.183.100.41:8888/P3API/P3API/GetProductAccount?product=32&accountNo=M512272 application/json 0 - 200 - text/plain;+charset=utf-8 4.3464ms`
        ///           extracted action result: `GetProductAccount`
        ///           
        /// Pattern5: pattern: @"(?<=:\s)[^:]+?(?=:\s+""Set-Cookie)"
        ///           source: P3 API log
        ///           match example: `2025-02-28 16:13:14.703 [default task-15137] WARN: Invalid cookie header: ""Set-Cookie: AWSALBTG=IzPDqzE1T+PHx6LivCjrrgZ2Aa4IEaKqJOytSdukpkwhkeU+db/512xP0e1JXdWgrRkSI8KNEDeQmuUak39T0JZSDG8TpmiCsWVYNq2KxNkX2DQU8npQFQReaB/3y2tv0p7WxwM5pliyBoEMVcwZ2Qbn0MUMquJqo7k6zml0YFXW; Expires=Fri, 07 Mar 2025 08:13:14 GMT; Path=/"". Invalid 'expires' attribute: Fri, 07 Mar 2025 08:13:14 GMT`
        ///           extracted action result: `Invalid cookie header`     
        ///           
        /// Pattern6: pattern: @"(?<=Action:)[^;]+"
        ///           source: P3 SMS API log
        ///           match example: `@"2025-03-20 20:51:34.964 +08:00 [ERR] User:;Action:P1_SetActivated_HKStock;Status:failure;"`
        ///           extracted action result: `P1_SetActivated_HKStock`     
        /// </summary>
        public enum APIActionPattern { Pattern1, Pattern2, Pattern3, Pattern4, Pattern5, Pattern6 };
        public APIActionParser() : base()
        {
            Patterns = new List<string>() { @"\b[\w\.]+\b(?=\s*=>)", 
                @"(?<=Function name calling:\s)\b\w+\b(?=\s=>)", 
                @"[A-Za-z0-9_]+\.[A-Za-z0-9_]+(?=\-\>)", 
                @"(?<=/)[A-Za-z]+(?=\?|$)", 
                @"(?<=:\s)[^:]+?(?=:\s+""Set-Cookie)",
                @"(?<=Action:)[^;]+"};
        }

        /// <summary>
        /// Use a specific pattern to extract timestamp
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="apiAction"></param>
        /// <returns></returns>
        public string parse(ref string logEntry, APIActionPattern apiActionPattern)
        {
            // Extract the timestamp using Regex
            if (Patterns.Count > (int)apiActionPattern)
            {
                string pattern = Patterns[(int)apiActionPattern];
                string apiAction = parse(ref logEntry, pattern);
                if (apiAction.Length > 0) return apiAction;
            }

            return "";
        }

    }
}

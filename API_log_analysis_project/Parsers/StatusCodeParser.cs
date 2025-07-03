using System.Text.RegularExpressions;

namespace API_log_analysis_project.Parsers
{
    public class StatusCodeParser : BaseParser
    {
        /// <summary>
        /// Pattern1: @"(?:Status(?: Code of HttpResponse)?|code)[=:]\s*(\d+)"
        ///           source: P3 API log
        ///           match example: `2025-02-28 16:13:35.316 [default task-15133] INFO: CommunityService.Function name: callGetApi - Function name calling: getNotifications => AcctNo:pIoYqsYsvLSorHCRLH2Ilg==; Status Code of HttpResponse: 200, body = {"meta":[{"code":200,"message":"Success"}],"data":{"total_page":1,"total_unread_noti":0,"responses":[]}}`
        ///           extract statusCode result: 200
        /// 
        /// Pattern2: pattern:  @"(?<=\""code\"":\s*)(\d+)"
        ///           source: P3 API log
        ///           match example: `2025-02-28 16:13:35.283 [default task-15105] INFO: SettingsServiceProxy.checkNotificationDevice => RESULT: [ BA17ED9C-387B-4633-8775-9E7C6B7DBB6F, 4846B022-F4C3-455C-9AC9-24C3A6F64884, 3.1.9, 18.3.1, D11C2939-6A72-4A27-B8B4-1D3F5FEFAB83, 4, [14.0.158.32, 198.143.53.27], , PHK, 100021303, ] ReturnResult [msg=success, code=1]`
        ///           extracted statusCode result: 1
        /// 
        /// Pattern3: pattern:  @"(?<=\""code\"":\s*)(\d+)"
        ///           source: P3 SMS API log
        ///           match example1: `2025-02-28 16:13:35.316 [default task-15133] INFO: CommunityService.Function name: callGetApi - Function name calling: getNotifications => AcctNo:pIoYqsYsvLSorHCRLH2Ilg==; Status Code of HttpResponse: 200, body = {"meta":[{"code":200,"message":"Success"}],"data":{"total_page":1,"total_unread_noti":0,"responses":[]}}`
        ///           extracted statusCode result: 200
        /// </summary>


        public enum StatusCodePattern { Pattern1, Pattern2, Pattern3 };
        public StatusCodeParser() : base()
        {
            // Patterns = new List<string>() { @"(?<=(?:AcctNo|accountNo):\s*)(?:[A-Za-z]+\d+|\d+)", @"(?<=accountNo=)[^&\s]+", @"(?<=[?&](?:accountNo|AcctNo|AccountList)=)[^&\s]+" };
            Patterns = new List<string>() { @"(?:Status(?: Code of HttpResponse)?|code)[=:]\s*(\d+)", @"(?<=meta=\[Meta{code=)(\d+)", @"(?<=\""code\"":\s*)(\d+)" };
        }

        /// <summary>
        /// Use a specific pattern to extract timestamp
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="apiAction"></param>
        /// <returns></returns>
        /// 
        public override string parse(ref string logEntry)
        {
            foreach (string pattern in Patterns)
            {
                Match match = Regex.Match(logEntry, pattern);
                if (match.Success)
                {
                    // Extract the status code from the first group
                    string statusCode = match.Groups[1].Success ? match.Groups[1].Value : string.Empty;
                    // Replace the matched pattern with an empty string
                    logEntry = Regex.Replace(logEntry, pattern, "");
                    // Return the extracted status code
                    return statusCode;
                }
            }

            return "";
        }
        public string parse(ref string logEntry, StatusCodePattern statuscodePattern)
        {
            // Extract the timestamp using Regex
            if (Patterns.Count > (int)statuscodePattern)
            {
                string pattern = Patterns[(int)statuscodePattern];
                string statuscode = parse(ref logEntry, pattern);
                if (statuscode.Length > 0) return statuscode;
            }

            return "";
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Parsers
{
    public class MessageLevelParser: BaseParser
    {
        public enum MessageLevelPattern { Pattern1, Pattern2 };

        /// <summary>
        /// Pattern1: @"\b(INFO|WARN|ERROR)\b(?=:)"
        ///           source: P3 API log
        ///           match example: `2025-02-28 16:13:35.316 [default task-15133] INFO: CommunityService.Function name: callGetApi - Function name calling: getNotifications => AcctNo:pIoYqsYsvLSorHCRLH2Ilg==; Status Code of HttpResponse: 200, body = {"meta":[{"code":200,"message":"Success"}],"data":{"total_page":1,"total_unread_noti":0,"responses":[]}}`
        ///           extract statusCode result: 200
        /// 
        /// Pattern2: pattern:  @"\b[A-Z]{3}(?=\])"
        ///           source: P3 SMS API log
        ///           match example: `2025-05-07 20:48:17.896 +08:00 [ERR] User:;Action:P1_SetActivated_HKStock;Status:failure;`
        ///           extracted statusCode result: 1
        /// 
        /// </summary>

        public MessageLevelParser() : base()
        {
            Patterns = new List<string>() { @"\b(INFO|WARN|ERROR)\b(?=:)", @"\b[A-Z]{3}(?=\])" };
        }

        /// <summary>
        /// Use a specific pattern to extract timestamp
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="messageLevelPattern"></param>
        /// <returns></returns>
        public string parse(ref string logEntry, MessageLevelPattern messageLevelPattern)
        {
            // Extract the timestamp using Regex
            if (Patterns.Count > (int)messageLevelPattern)
            {
                string pattern = Patterns[(int)messageLevelPattern];
                string messageLevel = parse(ref logEntry, pattern);
                if (messageLevel.Length > 0) return messageLevel;
            }

            return "";
        }

    }
}

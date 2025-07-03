using System.Text.RegularExpressions;

namespace API_log_analysis_project.Parsers
{
    /// <summary>
    /// InputOutput parser is used to determine if the record is a request or response record.
    /// </summary>
    public class InputOutputParser : BaseParser
    {
        /// <summary>
        /// Pattern1: pattern: @"(?<=\=\>\s)(INPUT|RESULT)(?=\:)"
        ///           source: P3 API log
        ///           match example1: `2025-02-28 16:15:17.336 [default task-15128] INFO: SettingsServiceProxy.serverTime => INPUT: [ 33591A9C-1F39-4F78-89C8-901CD27DD29F, 3.1.9, 2, PHK, ]`
        ///           extract accode result1: INPUT
        ///           match example2: `2025-02-28 16:15:17.336 [default task-15128] INFO: SettingsServiceProxy.serverTime => RESULT: [ 33591A9C-1F39-4F78-89C8-901CD27DD29F, 3.1.9, 2, PHK, ] ServerTimeResult [sTime=1740730517]`
        ///           extract accode result2: RESULT
        ///           description: After `INPUT`(request) or `RESULT`(response) has been extracted, then we can filter one of the identical record
        /// </summary>

        public enum InputOutputPattern { Pattern1 };
        public InputOutputParser() : base()
        {
            Patterns = new List<string>() { @"(?<=\=\>\s)(INPUT|RESULT)(?=\:)" };
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
            // keep log value to avoid missing parameter
            foreach (string pattern in Patterns)
            {
                Match match = Regex.Match(logEntry, pattern);
                if (match.Success)
                {
                    return match.Value.Trim();
                }
            }
            return "";
        }
        public string parse(ref string logEntry, InputOutputPattern inputOutputPattern)
        {
            // Extract the timestamp using Regex
            if (Patterns.Count > (int)inputOutputPattern)
            {
                string pattern = Patterns[(int)inputOutputPattern];
                string inputOrResult = parse(ref logEntry, pattern);
                if (inputOrResult.Length > 0) return inputOrResult;
            }

            return "";
        }

    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API_log_analysis_project.Parsers
{
    public class BaseParser
    {
        protected List<string> Patterns { get; set; } = new List<string>();

        /// <summary>
        /// This method traverse the pattern list, and get the longest pattern match
        /// </summary>
        /// <param name="logEntry"></param>
        /// <returns></returns>
        public virtual string parse(ref string logEntry)
        {
            string result = "";
            foreach (var pattern in Patterns)
            {
                string tmpResult = parse(logEntry, pattern);
                if (tmpResult.Length > 0 && tmpResult.Length > result.Length) result = tmpResult; // Get longest match
            }
            // Console.WriteLine("Extracted Value: " + result);
            if (result.Length > 0) logEntry = logEntry.Replace(result, "");
            // Console.WriteLine("Cleaned Log Entry: " + logEntry);

            return result;
        }

        private string parse(string logEntry, string pattern) // Pass by value version
        {
            // Extract the timestamp using Regex
            Match match = Regex.Match(logEntry, pattern);

            if (match.Success)
            {
                //// Console.WriteLine("Extracted Value: " + match.Value);

                // Remove the timestamp from the log entry
                logEntry = Regex.Replace(logEntry, pattern, "");

                //// Console.WriteLine("Cleaned Log Entry: " + logEntry);
                return match.Value.Trim();
            };

            return "";
        }

        public string parse(ref string logEntry, string pattern) // Pass by ref version
        {
            // Extract the timestamp using Regex
            Match match = Regex.Match(logEntry, pattern);
            
            if (match.Success)
            {
                // Console.WriteLine("Extracted Value: " + match.Value);

                // Remove the timestamp from the log entry
                logEntry = Regex.Replace(logEntry, pattern, "");
                // Console.WriteLine("Cleaned Log Entry: " + logEntry);
                return match.Value.Trim();
            };

            return "";
        }
    }
}

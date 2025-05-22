using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API_log_analysis_project.Parsers
{
    /// <summary>
    /// Pattern 1: 2025-02-28 15:58:43.468
    /// Pattern 2: 2025-02-28 15:58:43.468 +08:00
    /// </summary>
    public enum TimestampPattern { Pattern1, Pattern2 };
    public class TimestampParser: BaseParser
    {
        public TimestampParser() : base()
        {
            Patterns = new List<string>() { @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}", @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}" };
        }
        
        
        ///// <summary>
        ///// Use all existing pattern to extract timestamp one by one
        ///// </summary>
        ///// <param name="logEntry"></param>
        ///// <returns></returns>
        //public string parse(ref string logEntry)
        //{
        //    foreach (var pattern in Patterns)
        //    {
        //        string timestamp = parse(ref logEntry, pattern);
        //        if (timestamp.Length > 0)
        //        {
        //            if (DateTime.TryParse(timestamp, out DateTime result)) return timestamp;
        //        }
        //    }

        //    return "";
        //}

        /// <summary>
        /// Use all existing pattern to extract timestamp one by one
        /// </summary>
        /// <param name="logEntry"></param>
        /// <returns></returns>
        public DateTime? parseToDateTime(ref string logEntry)
        {
            foreach (var pattern in Patterns)
            {
                string timestamp = parse(ref logEntry, pattern);
                if (timestamp.Length > 0)
                {
                    if (DateTime.TryParse(timestamp, out DateTime result)) return result;
                }
            }

            return null;
        }

        ///// <summary>
        ///// Use a specific pattern to extract timestamp
        ///// </summary>
        ///// <param name="logEntry"></param>
        ///// <param name="timestampPattern"></param>
        ///// <returns></returns>
        //public string parse(ref string logEntry, TimestampPattern timestampPattern)
        //{
        //    // Extract the timestamp using Regex
        //    if (Patterns.Count > (int)timestampPattern) {
        //        string pattern = Patterns[(int)timestampPattern];
        //        string timestamp = parse(ref logEntry, pattern);
        //        if (timestamp.Length > 0)
        //        {
        //            if (DateTime.TryParse(timestamp, out DateTime result)) return timestamp;
        //        }
        //    }

        //    return "";
        //}



    }
}

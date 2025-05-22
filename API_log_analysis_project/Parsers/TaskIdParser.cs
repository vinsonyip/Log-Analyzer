using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Parsers
{
    public class TaskIdParser : BaseParser
    {
        public enum TaskIdPattern { Pattern1 };
        public TaskIdParser() : base()
        {
            Patterns = new List<string>() { @"(?<=\[)default task-\d+(?=]\s+[A-Z]+:)" };
        }

        /// <summary>
        /// Use a specific pattern to extract timestamp
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="messageLevelPattern"></param>
        /// <returns></returns>
        public string parse(ref string logEntry, TaskIdPattern taskIdPattern)
        {
            // Extract the timestamp using Regex
            if (Patterns.Count > (int)taskIdPattern)
            {
                string pattern = Patterns[(int)taskIdPattern];
                string taskId = parse(ref logEntry, pattern);
                if (taskId.Length > 0) return taskId;
            }

            return "";
        }
    }
}

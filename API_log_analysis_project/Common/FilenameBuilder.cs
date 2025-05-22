using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Common
{
    internal class FilenameBuilder
    {
        // LPR => Log Parsing Report
        public static string LPRFilenamePrefix { get; } = "LogParsingReport_";
        public static string LPRFileExt { get; } = ".json";

        public static string GetLogParsingReportName(string dateTime)
        {
            return "";
        }
    }
}

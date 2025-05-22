using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Common
{
    public class GlobalStore
    {
        private static string _defaultDirPath = Path.Combine(Directory.GetCurrentDirectory(), "logs_analysis");

        private static string _rootDirPath = _defaultDirPath;

        public static string ReportDirPath = Path.Combine(_rootDirPath, "reports");
        public static string ParsingResultOutputDirPath = Path.Combine(_rootDirPath, "output");

        public static readonly string InfluxDB_P3SMSAPILogMeasureName = "P3_SMS_API_application_logs_test";
        public static readonly string InfluxDB_P3APILogMeasureName = "P3_API_application_logs_test";
        public static void ResetGlobalRootDirPath()
        {
            _rootDirPath = _defaultDirPath;
            ReportDirPath = Path.Combine(_rootDirPath, "reports");
            ParsingResultOutputDirPath = Path.Combine(_rootDirPath, "output");
        }

        public static bool SetGlobalRootDirPath(string rootDirPath)
        {
            try
            {
                if (Directory.Exists(rootDirPath))
                {
                    _rootDirPath = rootDirPath;
                }
                else
                {
                    Directory.CreateDirectory(rootDirPath);
                }

                _rootDirPath = rootDirPath;
                ReportDirPath = Path.Combine(_rootDirPath, "reports");
                ParsingResultOutputDirPath = Path.Combine(_rootDirPath, "output");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string GetGlobalRootDirPath() { 
            return _rootDirPath;
        }
    }
}

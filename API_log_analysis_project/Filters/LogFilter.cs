using API_log_analysis_project.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Filters
{
    public class LogFilter : ILogFilter
    {
        public bool IsCollectRequestLog { get; set; } = true;
        public bool IsCollectResponseLog { get; set; } = false; // Set this false, cuz response log is far longer than the request log


        public virtual bool execute(LogDataPoint logDataPoint)
        {
            return IsValid(logDataPoint) && IsIncluded(logDataPoint);
        }

        protected virtual bool IsIncluded(LogDataPoint logDataPoint)
        {
            if (IsCollectRequestLog)
            {
                if (logDataPoint.IsRequest != null)
                {
                    if ((bool)logDataPoint.IsRequest) return true;
                    else return false;
                }
            }

            if (IsCollectResponseLog)
            {
                if (logDataPoint.IsRequest != null)
                {
                    if (!(bool)logDataPoint.IsRequest) return true;
                    else return false;
                }
            }

            return true;
        }

        protected virtual bool IsValid(LogDataPoint logDataPoint)
        {
            if (logDataPoint.Level.Length <= 0 || logDataPoint.Timestamp == null) return false;
            return true;
        }
    }
}

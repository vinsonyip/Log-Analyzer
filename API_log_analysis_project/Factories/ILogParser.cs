using API_log_analysis_project.Entities;
using API_log_analysis_project.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Factories
{
    public interface ILogParser
    {
        public LogDataPoint? parse(string rawLog, ILogFilter? logFilter = null);
    }
}

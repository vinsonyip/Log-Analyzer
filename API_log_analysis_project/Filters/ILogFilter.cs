using API_log_analysis_project.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Filters
{
    public interface ILogFilter
    {
        public bool execute(LogDataPoint logDataPoint);
    }
}

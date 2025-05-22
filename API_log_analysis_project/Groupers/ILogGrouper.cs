using API_log_analysis_project.Entities;
using InfluxDB.Client.Writes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Groupers
{
    /// <summary>
    /// Log grouper is also like a buffer, which will collect all the data inside, and dispatch in case the size hit the batch size
    /// </summary>
    public interface ILogGrouper
    {
        public LogGrouperResult? Execute(LogDataPoint logDataPoint, int batchSize = 1); 

        public LogGrouperResult? GetRemainLogGrouperResult();
    }
}

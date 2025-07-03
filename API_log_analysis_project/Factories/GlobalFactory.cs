using API_log_analysis_project.Filters;
using API_log_analysis_project.Groupers;

namespace API_log_analysis_project.Factories
{
    public enum ParserName { P3_API_Parser, P3_SMS_API_Parser, SG_NGINX_LOG_Parser }
    public enum FilterName { P3_API_Filter, P3_SMS_API_Filter, SG_NGINX_LOG_Filter }
    public enum GrouperName { P3_API_Grouper, P3_SMS_API_Grouper, SG_NGINX_LOG_Grouper }
    public class GlobalFactory
    {
        /// <summary>
        /// Return P3APILogParser by default
        /// </summary>
        /// <param name="parserName"></param>
        /// <returns></returns>
        public static ILogParser GetParser(ParserName parserName)
        {
            ILogParser logParser;
            switch (parserName)
            {
                case ParserName.P3_API_Parser:
                    logParser = new P3APILogParser();
                    break;
                case ParserName.P3_SMS_API_Parser:
                    logParser = new P3SMSAPILogParser();
                    break;
                case ParserName.SG_NGINX_LOG_Parser:
                    logParser = new SGNginxLogParser();
                    break;
                default:
                    logParser = new P3APILogParser();
                    break;
            }

            return logParser;
        }

        /// <summary>
        /// Return P3APILogFilter by default
        /// </summary>
        /// <param name="parserName"></param>
        /// <returns></returns>
        public static ILogFilter GetFilter(FilterName filterName)
        {
            ILogFilter logFilter;

            switch (filterName)
            {
                case FilterName.P3_API_Filter:
                    logFilter = new P3APILogFilter();
                    break;
                case FilterName.P3_SMS_API_Filter:
                    logFilter = new P3SMSAPILogFilter();
                    break;
                case FilterName.SG_NGINX_LOG_Filter:
                    logFilter = new SGNginxLogFilter();
                    break;
                default:
                    logFilter = new P3APILogFilter();
                    break;
            }

            return logFilter;
        }

        /// <summary>
        /// Return P3APILogFilter by default
        /// </summary>
        /// <param name="parserName"></param>
        /// <returns></returns>
        public static ILogGrouper GetGrouper(GrouperName grouperName)
        {
            ILogGrouper logGrouper;

            switch (grouperName)
            {
                case GrouperName.P3_API_Grouper:
                    logGrouper = new P3APILogGrouper();
                    break;
                case GrouperName.P3_SMS_API_Grouper:
                    logGrouper = new P3SMSAPILogGrouper();
                    break;
                case GrouperName.SG_NGINX_LOG_Grouper:
                    logGrouper = new SGNginxLogGrouper();
                    break;
                default:
                    logGrouper = new P3APILogGrouper();
                    break;
            }

            return logGrouper;
        }
    }
}

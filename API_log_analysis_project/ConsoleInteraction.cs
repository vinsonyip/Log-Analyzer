using API_log_analysis_project.Common;
using API_log_analysis_project.Common.UI;
using API_log_analysis_project.DBWriters;
using API_log_analysis_project.Entities;
using API_log_analysis_project.Factories;
using API_log_analysis_project.Filters;
using API_log_analysis_project.Groupers;
using API_log_analysis_project.Trackers;

namespace API_log_analysis_project
{
    public class ConsoleInteraction
    {


        private LogParsingTracker _tracker;
        private IDBWriter _dbWriter = new DBWriterV2();
        public ConsoleInteraction(LogParsingTracker tracker)
        {
            _tracker = tracker;
            _dbWriter = new DBWriterV2();
        }

        public LogParsingTracker GetLogParsingTracker()
        {
            return _tracker;
        }

        public void InitializedGlobalRootDirPath()
        {
            if (!UserTrueFalseInteraction("\nWould you like to proceed with default directory? (Y/N)"))
            {
                Console.Write("\nPlease input the root directory path:");
                string rootDirPath = Console.ReadLine() ?? "";
                rootDirPath = rootDirPath.Trim();
                if (GlobalStore.SetGlobalRootDirPath(rootDirPath)) 
                {
                    Console.Write("\n[INFO] New root directory path setup successfully!\n");
                }
                else
                {
                    Console.Write("\n[ERR] New root directory path setup fail!\n");
                }
            }
        }

        public void SetLogTracker(LogParsingTracker tracker)
        {
            _tracker = tracker;
        }

        public async Task ProcessLogs()
        {
            string logDirectoryPath = GlobalStore.GetGlobalRootDirPath();

            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
                Console.WriteLine($"\n[WARN] Root log directory is not exist... Created path: {logDirectoryPath}");
                Console.WriteLine($"[WARN] Please put logs into the create folder\n");
                return;
            }
            string[] logFilePaths = Directory.GetFiles(logDirectoryPath, "*.log", SearchOption.TopDirectoryOnly);

            // Parse & write log files to DB by async/await
            //string rootPath = logDirectoryPath;
            //List<string> logFilePaths = new List<string>() { $"{rootPath}\\gmobile-global-2025-02-28-33-extract.log", $"{rootPath}\\gmobile-global-2025-02-28-33-extract2.log" };

            await ProcessListOfLog(logFilePaths.ToList());
            Console.WriteLine("\nAll logs processed concurrently!\n\n"); // This is a console.writeln, use this function to avoid writing to the progress bar.
        }

        public async Task ProcessSelectedLogs()
        {
            string[] logFilePaths = Directory.GetFiles(GlobalStore.GetGlobalRootDirPath(), "*.log", SearchOption.TopDirectoryOnly);
            Console.WriteLine("\n============ Found the following logs =============");
            for (int i = 0; i < logFilePaths.Length; i++)
            {
                Console.WriteLine($"{i}: {logFilePaths[i]}");
            }
            Console.WriteLine("=========================");
            Console.Write("\nYour choice (Please use \",\" as separator (e.g. 1,2,3), (e.g. 1-3,5,7):");
            string logFileSelected = Console.ReadLine() ?? "";

            if (logFileSelected.Length <= 0 || logFilePaths.Length <= 0)
            {
                Console.WriteLine("[WARN] Can't detect any input, return to previous page");
                return;
            }

            string[] logFileSelectedId = logFileSelected.Split(",");

            List<string> selectedLogFiles = new List<string>();

            foreach (string logFileId in logFileSelectedId)
            {
                if (int.TryParse(logFileId, out int result))
                {
                    selectedLogFiles.Add(logFilePaths[result]);
                }
                else
                {
                    string[] idSplit = logFileId.Split("-");
                    if (idSplit.Length != 2)
                    {
                        Console.WriteLine($"Can't parse logFileId:{logFileId}\n");
                        return;
                    }

                    string startingIndex = idSplit[0];
                    string endingIndex = idSplit[1];

                    if (int.TryParse(startingIndex, out int startingIndexInt) &&
                        int.TryParse(endingIndex, out int endingIndexInt))
                    {
                        if (startingIndexInt > endingIndexInt)
                        {
                            Console.WriteLine($"[ERR]starting index should not bigger than ending index\n");
                            return;
                        }

                        if (startingIndexInt >= logFilePaths.Length)
                        {
                            Console.WriteLine($"[ERR]starting index should not bigger than file list size\n");
                            return;
                        }

                        if (endingIndexInt >= logFilePaths.Length)
                        {
                            Console.WriteLine($"[ERR]ending index should not bigger than file list size\n");
                            return;
                        }
                        int totalRecordCount = endingIndexInt - startingIndexInt + 1;

                        selectedLogFiles.AddRange(logFilePaths.ToList().GetRange(startingIndexInt, totalRecordCount));
                    }
                }
            }

            await ProcessListOfLog(selectedLogFiles, isOutputProfileForProvidedLogPathsOnly: true);

            //_tracker.UpdateLogProfileToReport();
        }

        // ========= Common utils below ==========

        /// <summary>
        /// This function will config the log profile(A kind of config profile), so we can use the config to parse the log.
        /// </summary>
        /// <param name="logPaths"></param>
        /// <param name="isOutputProfileForProvidedLogPathsOnly"></param>
        /// <returns></returns>
        private List<LogProfile> GenerateLogProfiles(string[] logPaths, bool isOutputProfileForProvidedLogPathsOnly = false)
        {
            List<LogProfile> logProfiles = new();

            foreach (string logFilePath in logPaths)
            {

                LogProfile logProfile = new LogProfile();
                logProfile.LogFilePath = logFilePath;
                logProfile.LogName = Path.GetFileNameWithoutExtension(logFilePath);

                //Select log parser
                if (logProfile.LogName.Contains(@"gmobile-global") || logProfile.LogName.Contains(@"gmobile-st"))
                {
                    logProfile.Parser = ParserName.P3_API_Parser;
                    logProfile.LogParser = GlobalFactory.GetParser(ParserName.P3_API_Parser);
                    logProfile.LogFilter = GlobalFactory.GetFilter(FilterName.P3_API_Filter);
                    logProfile.LogGrouper = GlobalFactory.GetGrouper(GrouperName.P3_API_Grouper);
                }// define gmobile-st as P3_API_Parser
                else if (logProfile.LogName.Contains(@"sms"))
                {
                    logProfile.Parser = ParserName.P3_SMS_API_Parser;
                    logProfile.LogParser = GlobalFactory.GetParser(ParserName.P3_SMS_API_Parser);
                    logProfile.LogFilter = GlobalFactory.GetFilter(FilterName.P3_SMS_API_Filter);
                    logProfile.LogGrouper = GlobalFactory.GetGrouper(GrouperName.P3_SMS_API_Grouper);
                }
                else if (logProfile.LogName.Contains(@"sg-nginx"))
                {
                    logProfile.Parser = ParserName.SG_NGINX_LOG_Parser;
                    logProfile.LogParser = GlobalFactory.GetParser(ParserName.SG_NGINX_LOG_Parser);
                    logProfile.LogFilter = GlobalFactory.GetFilter(FilterName.SG_NGINX_LOG_Filter);
                    logProfile.LogGrouper = GlobalFactory.GetGrouper(GrouperName.SG_NGINX_LOG_Grouper);
                }
                else
                {
                    logProfile.Parser = ParserName.P3_SMS_API_Parser;
                    logProfile.LogParser = GlobalFactory.GetParser(ParserName.P3_SMS_API_Parser);
                    logProfile.LogFilter = GlobalFactory.GetFilter(FilterName.P3_SMS_API_Filter);
                    logProfile.LogGrouper = GlobalFactory.GetGrouper(GrouperName.P3_SMS_API_Grouper);
                }


                logProfiles.Add(logProfile);
            }

            _tracker.SyncLogProfiles(logProfiles);

            if (isOutputProfileForProvidedLogPathsOnly)
            {
                return _tracker.GetLogSelectedProfiles(logProfiles);
            }

            return _tracker.GetLogProfiles();
        }

        /// <summary>
        /// This function auto-detect which parser to use, and apply it to the log automatically
        /// </summary>
        /// <param name="logProfiles"></param>
        /// <returns></returns>
        private async Task<LogProfile[]> ProcessListOfLog(List<string> logPaths, bool isOutputProfileForProvidedLogPathsOnly = false)
        {
            _tracker.SyncLogProfiles(GenerateLogProfiles(logPaths.ToArray(), isOutputProfileForProvidedLogPathsOnly: isOutputProfileForProvidedLogPathsOnly)); // Sync the log profiles of the log parsing tracker
       
            Task<LogProfile>[] processingTasks = new Task<LogProfile>[_tracker.GetLogProfiles().Count];

            bool isWriteToTxt = UserTrueFalseInteraction("Write as Line Protocal to .txt format (Y), OR write to InfluxDB (N)?:");

            if (isWriteToTxt)
            {
                Console.WriteLine("Writing to .txt now ...");
            }
            else
            {
                Console.WriteLine("Writing to InfluxDB now ...");
            }
           
            await _dbWriter.CheckAndCreateBucketIfNotExist();

            for (int i = 0; i < _tracker.GetLogProfiles().Count; i++)
            {
                LogProfile logProfile = _tracker.GetLogProfiles()[i];
                //DBWriterV2 tmpDBWriter = new DBWriterV2();
                //ILogParser logParser = GlobalFactory.GetParser(logProfile.Parser);
                //ILogFilter logFilter = new P3APILogFilter { IsCollectRequestLog = true, IsCollectResponseLog = false };
                //ILogGrouper logGrouper = new P3APILogGrouper();

                //processingTasks[i] = tmpDBWriter.WriteLogToTxtAsync(logProfile, logParser, logFilter, logGrouper, _tracker);
                if(isWriteToTxt)
                {
                    processingTasks[i] = _dbWriter.WriteLogToTxtAsync(logProfile, _tracker);
                }
                else
                {
                    processingTasks[i] = _dbWriter.WriteLogToInfluxDBAsync(logProfile, _tracker);
                }
                
            }
            LogProfile[] logProfileResult = await Task.WhenAll(processingTasks);
            //_tracker.UpdateLogProfileToReport();

            return logProfileResult;
        }

        public static bool UserTrueFalseInteraction(string message)
        {
            // User interaction
            while (true)
            {
                Console.Write(message);
                string? TrueFalseStr = Console.ReadLine();

                if (TrueFalseStr != null)
                {
                    Console.Clear();  // Clears entire console buffer
                    if (TrueFalseStr.ToUpper() == "Y")
                    {
                        return true;
                    }
                    else if (TrueFalseStr.ToUpper() == "N")
                    {
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("\nThe input can't be detected, please try again.\n");
                    }
                }
            }
            
        }
    }
}

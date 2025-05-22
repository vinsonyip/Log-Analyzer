using API_log_analysis_project.Common;
using API_log_analysis_project.Entities;
using System.Text.Json;

namespace API_log_analysis_project.Trackers
{
    /// <summary>
    /// This is a Singleton class
    /// </summary>
    public class LogParsingTracker: ILogParsingTracker
    {
        private LogParsingReport report;

        private Dictionary<string, int> _logNameToProfileIndexMap = new Dictionary<string, int>();

        private static LogParsingTracker? _logParsingTracker = null;
        private LogParsingTracker()
        {
            report = new LogParsingReport();
        }

        public static LogParsingTracker GetInstance()
        {
            if(_logParsingTracker == null)
            {
                _logParsingTracker= new LogParsingTracker();
            }
            return _logParsingTracker;
        }

        public void StartParsingProcess()
        {
            LoadTrackingReport();
            report.StartAt = DateTime.Now;
        }

        /// <summary>
        /// 1. Update `endAt` datetime of the report
        /// 2. Export the report to default dir.
        /// </summary>
        public void StopParsingProcess()
        {
            
            for (int i = 0; i < report.Profiles.Count; i++) {
                if (report.Profiles[i].Status != "D" || (report.Profiles[i].Status == "P" && report.Profiles[i].LastLine > 0)) report.Profiles[i].Status = "E";
                if (report.Profiles[i].ProgressPercent == 100) { 
                    report.Profiles[i].Status = "D";
                    report.Profiles[i].Message = "Success";
                }
            }

            report.EndAt = DateTime.Now;
            ExportTrackingReport();
        }

        public void LoadTrackingReport()
        {
            string reportExportDirPath = GlobalStore.ReportDirPath;

            if (!Directory.Exists(reportExportDirPath)) return;

            string[] reportFiles = Directory.GetFiles(reportExportDirPath, $"{FilenameBuilder.LPRFilenamePrefix}*{FilenameBuilder.LPRFileExt}", SearchOption.TopDirectoryOnly);

            if (reportFiles.Length > 0) Console.WriteLine("\n===\n Report files have been found \n===");
            else return;
            
            while (true)
            {
                
                if (ConsoleInteraction.UserTrueFalseInteraction("\nWould you like to proceed with existing report? (Y/N)"))
                {
                    if (reportFiles.Length > 0)
                    {
                        Console.WriteLine("\n=== The latest log report will be selected ===\n");
                        DateTime latestReportDateTime = DateTime.MinValue;
                        string latestReportDateTimeStr = "";

                        foreach (string reportFile in reportFiles)
                        {
                            string filenameWithoutExt = Path.GetFileNameWithoutExtension(reportFile);
                            string reportTimestamp = filenameWithoutExt.Replace(FilenameBuilder.LPRFilenamePrefix, "");
                            DateTime reportDateTime = DateTime.FromFileTimeUtc(long.Parse(reportTimestamp));
                            if (reportDateTime > latestReportDateTime)
                            {
                                latestReportDateTime = reportDateTime;
                                latestReportDateTimeStr = reportTimestamp;
                            }
                        }

                        string latestReportFilePath = "";
                        if (latestReportDateTime <= DateTime.MinValue)
                        {
                            Console.WriteLine("\nThe latest report can't be found! Please try again\n");
                        }
                        else
                        {
                            latestReportFilePath = Path.Combine(reportExportDirPath, $"{FilenameBuilder.LPRFilenamePrefix}{latestReportDateTimeStr}{FilenameBuilder.LPRFileExt}");
                            var tmpReport = JsonSerializer.Deserialize<LogParsingReport>(File.ReadAllText(latestReportFilePath));
                            if (tmpReport != null)
                            {
                                report = tmpReport;
                                return;
                            }
                        }
                    }

                    Console.WriteLine("\nThe log parsing report can't be parsed! Please try again\n");
                }
                else
                {
                    return;
                }

            }
        }

        public void ExportTrackingReport()
        {
            string reportExportDirPath = GlobalStore.ReportDirPath;
            if (!Directory.Exists(reportExportDirPath)) Directory.CreateDirectory(reportExportDirPath);
            string fileNameOutput = Path.Combine(reportExportDirPath, $"LogParsingReport_{report.EndAt.ToFileTimeUtc()}.json");
            string jsonString = JsonSerializer.Serialize(report);
            File.WriteAllText(fileNameOutput, jsonString);
        }

        public void UpdateLogProfileToReport(LogProfile logProfile)
        {
            bool isProfileExist = false;

            if (_logNameToProfileIndexMap.TryGetValue(logProfile.LogName, out int index))
            {
                report.Profiles[index] = logProfile;
                isProfileExist = true;
            }
            else
            {
                for (int i = 0; i < report.Profiles.Count; i++)
                {
                    if (report.Profiles[i].LogName == logProfile.LogName)
                    {
                        report.Profiles[i] = logProfile;
                        isProfileExist = true;

                        _logNameToProfileIndexMap.Add(logProfile.LogName, i);


                        break;
                    }
                }
            }

            if (!isProfileExist)
            {
                report.Profiles.Add(logProfile);
            }
        }

        public void SetMessageToReport(string message)
        {
            report.Message = message;
        }
        /// <summary>
        /// This is going to be used before the parsing being started;
        /// When the parsing step start, the folder may have new log file which don't have the existing log profile, we need to add them to the tracking by using this method.
        /// </summary>
        /// <param name="logProfileToBeUpdated"></param>
        public void SyncLogProfiles(List<LogProfile> logProfileToSync)
        {
            // This list stores all the log profile to be processed, cuz some of the existing log profile may not applicable to current log dir path, so we should remove them
            List<LogProfile> logProfileToBeProcessed = new List<LogProfile>();

            Dictionary<string, LogProfile> logNameToProfileMapForUpdate = new Dictionary<string, LogProfile>();

            foreach (LogProfile profile in report.Profiles)
            {
                logNameToProfileMapForUpdate[profile.LogName] = profile;
            }

            int index = 0;
            foreach (LogProfile profile in logProfileToSync)
            {
                if (logNameToProfileMapForUpdate.TryGetValue(profile.LogName, out LogProfile existingLogProfile))
                {
                    // LogParser, LogFilter and LogGrouper will not export to report, so we need to assign it to existing profile object in runtime
                    existingLogProfile.LogParser = profile.LogParser;
                    existingLogProfile.LogFilter = profile.LogFilter;
                    existingLogProfile.LogGrouper = profile.LogGrouper;

                    logProfileToBeProcessed.Add(existingLogProfile);
                }
                else // For the log profile can't be found from the `updatedLogProfile` dictionary
                {
                    logProfileToBeProcessed.Add(profile);
                }

                _logNameToProfileIndexMap[profile.LogName] = index;
                index += 1;
            }

            report.Profiles = logProfileToBeProcessed;
        }

        public List<LogProfile> GetLogProfiles() { 
            return report.Profiles;
        }

        public List<LogProfile> GetLogSelectedProfiles(List<LogProfile> selectedProfiles)
        {
            List<LogProfile> tmpLogProfiles = new List<LogProfile>();

            foreach (var item in selectedProfiles)
            {
                if (_logNameToProfileIndexMap.TryGetValue(item.LogName, out int index))
                {
                    tmpLogProfiles.Add(report.Profiles[index]);
                }
            }
            return tmpLogProfiles;
        }
    }
}

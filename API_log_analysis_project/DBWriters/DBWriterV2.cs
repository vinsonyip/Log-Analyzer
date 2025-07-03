using API_log_analysis_project.Common;
using API_log_analysis_project.Common.UI;
using API_log_analysis_project.Entities;
using API_log_analysis_project.Entities.Configs;
using API_log_analysis_project.Factories;
using API_log_analysis_project.Filters;
using API_log_analysis_project.Groupers;
using API_log_analysis_project.Trackers;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace API_log_analysis_project.DBWriters
{

    public class DBWriterV2: IDBWriter
    {

        public LogProfile _logProfile = new LogProfile();
        private static InfluxDBConfig _settings = InfluxDBConfig.GetInstance();
        private readonly int _batchSize = 10;

        public enum ExportDest { InfluxDB, Txt }

        public async Task CheckAndCreateBucketIfNotExist()
        {
            using var client = new InfluxDBClient(_settings.InfluxUrl, _settings.Token);
            var bucketAPI = client.GetBucketsApi();
            Bucket? bucket = await bucketAPI.FindBucketByNameAsync(_settings.Bucket);

            if (bucket == null) await bucketAPI.CreateBucketAsync(_settings.Bucket, _settings.OrgId);
            client.Dispose();
        }

        public async Task<LogProfile> WriteLogToInfluxDBAsync(LogProfile logProfile, ILogParsingTracker logParsingTracker)
        {
            _logProfile = logProfile;
            ILogParser? logParser = _logProfile.LogParser;
            ILogFilter? logFilter = _logProfile.LogFilter;
            ILogGrouper? logGrouper = _logProfile.LogGrouper;

            string outputPathName = _settings.InfluxUrl;
            using var client = new InfluxDBClient(_settings.InfluxUrl, _settings.Token);
            var writeApi = client.GetWriteApiAsync();


            if (_logProfile.Status == "D") return _logProfile; // Do nothing if the status = "D"

            _logProfile.StartAt = DateTime.Now;
            string logPath = _logProfile.LogFilePath;


            int currentLineRead = 0;
            int currentLineCompleted = 0;
            try
            {
                // == Progress bar ===
                var taskId = ProgressManager.RegisterTask();
                FileInfo fileInfo = new FileInfo(logPath);
                long totalBytes = fileInfo.Length;
                long processedBytes = (long)(_logProfile.ProgressPercent / 100 * totalBytes); // Calculate the latest progress
                string filename = fileInfo.Name;
                const int NewLineBytes = 1; // 1 for \n, 2 for \r\n
                ProgressManager.UpdateProgress(taskId, _logProfile.ProgressPercent / 100, filename);
                // == Progress bar ===


                currentLineRead = 0; // Use to track the current process line
                                     // Exec at the last breaking line
                                     // Append the log parsing result to the back of previous result file

                currentLineCompleted = _logProfile.LastLine;

                await foreach (string line in System.IO.File.ReadLinesAsync(logPath))
                {
                    if (currentLineRead < _logProfile.LastLine) // Skip the previously processed lines
                    {
                        currentLineRead += 1;
                        continue;
                    }
                    // == Progress bar ===
                    var lineBytes = Encoding.UTF8.GetByteCount(line) + NewLineBytes;
                    processedBytes += lineBytes;
                    var progress = Math.Min(100, (double)processedBytes / totalBytes * 100);
                    ProgressManager.UpdateProgress(taskId, progress, filename);
                    // == Progress bar ===

                    // Step1: Parse the log record
                    LogDataPoint? logDP = null;

                    if (logParser == null) throw new Exception("Log parser can't be null");

                    logDP = logParser.parse(line, logFilter);

                    if (logDP == null) throw new Exception("logDP can't be null");

                    // Step2: Group the log record
                    var logGrouperResult = logGrouper!.Execute(logDP, _batchSize);
                    if (logGrouperResult != null)
                    {
                        var logRecordListToInsertToInfluxDB = logGrouperResult.PointDataFlushList;
                        var logGrouperInfo = logGrouperResult.LogDataPointFlushList;

                        // Step3: Flush the log record to InfluxDB
                        if (logRecordListToInsertToInfluxDB != null)
                        {
                            if (logDP != null && _settings.IsEnabled && logDP.Timestamp != null)
                            {
                                //// Test: Exception problem, test tracking
                                //if (currentLineCompleted == 5000) throw new Exception();
                                await writeApi.WritePointsAsync(logRecordListToInsertToInfluxDB, _settings.Bucket, _settings.Org);

                                currentLineCompleted = currentLineRead;

                                // === Update log profile ===
                                _logProfile.OutputPath = outputPathName;
                                _logProfile.EndAt = DateTime.Now;
                                _logProfile.LastLine = currentLineCompleted;
                                _logProfile.ProgressPercent = progress;

                                LogParsingTracker.GetInstance().UpdateLogProfileToReport(_logProfile);
                                // === Update log profile ===
                            }
                        }
                    }
                    currentLineRead += 1;

                }

                //var remainingLogRecordInFlushList = logGrouper.GetRemainDataPointsIfExists() ?? [];
                var remainLogGrouperResult = logGrouper!.GetRemainLogGrouperResult();

                if (remainLogGrouperResult == null) throw new Exception("remainLogGrouperResult can't be null");
                
                var remainLogRecordListToInsertToInfluxDB = remainLogGrouperResult.PointDataFlushList;
                //var remainLogGrouperInfo = remainLogGrouperResult.LogDataPointFlushList;

                await writeApi.WritePointsAsync(remainLogRecordListToInsertToInfluxDB, _settings.Bucket, _settings.Org);
                currentLineCompleted = currentLineRead;

                // === Update log profile ===
                _logProfile.ProgressPercent = 100;
                _logProfile.OutputPath = outputPathName;
                _logProfile.EndAt = DateTime.Now;
                _logProfile.LastLine = currentLineCompleted;

                LogParsingTracker.GetInstance().UpdateLogProfileToReport(_logProfile);
                // === Update log profile ===


                ProgressManager.UpdateProgress(taskId, 100, filename);


            }
            catch (Exception ex)
            {
                _logProfile.Status = "E";
                _logProfile.Message = ex.Message;
            }
            finally
            {
                _logProfile.OutputPath = outputPathName;
                _logProfile.EndAt = DateTime.Now;
                _logProfile.LastLine = currentLineCompleted;
                LogParsingTracker.GetInstance().UpdateLogProfileToReport(_logProfile);
            }

            return _logProfile;
        }

        public async Task<LogProfile> WriteLogToTxtAsync(LogProfile logProfile, ILogParsingTracker logParsingTracker)
        {

            _logProfile = logProfile;
            ILogParser? logParser = _logProfile.LogParser;
            ILogFilter? logFilter = _logProfile.LogFilter;
            ILogGrouper? logGrouper = _logProfile.LogGrouper;

            if (logParser == null) throw new Exception("logParser can't be null");
            if (logFilter == null) throw new Exception("logFilter can't be null");
            if (logGrouper == null) throw new Exception("logGrouper can't be null");

            if (_logProfile.Status == "D") return _logProfile; // Do nothing if the status = "D"

            _logProfile.StartAt = DateTime.Now;
            string logPath = _logProfile.LogFilePath;

            string filePath = logPath.Replace(".log", "-output.log");
            string fileName = Path.GetFileName(logPath).Replace(".log", "-output.log");

            string outputDir = GlobalStore.ParsingResultOutputDirPath;

            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string outputFilePath = Path.Combine(outputDir, fileName);

            int currentLineRead = 0;
            int currentLineCompleted = 0;
            try
            {
                // == Progress bar ===
                var taskId = ProgressManager.RegisterTask();
                FileInfo fileInfo = new FileInfo(logPath);
                long totalBytes = fileInfo.Length;
                long processedBytes = (long)(_logProfile.ProgressPercent / 100 * totalBytes);
                string filename = fileInfo.Name;
                const int NewLineBytes = 1; // 1 for \n, 2 for \r\n
                ProgressManager.UpdateProgress(taskId, _logProfile.ProgressPercent / 100, filename);
                // == Progress bar ===

                currentLineRead = 0; // Use to track the current process line
                                     // Exec at the last breaking line
                                     // Append the log parsing result to the back of previous result file

                currentLineCompleted = _logProfile.LastLine;

                if (_logProfile.LastLine == 0) //For this case, the log output file should be started from 0, so delete existing output.
                {
                    System.IO.File.Delete(outputFilePath);
                }

                await foreach (string line in System.IO.File.ReadLinesAsync(logPath))
                {
                    if (currentLineRead < _logProfile.LastLine)
                    {
                        currentLineRead += 1;
                        continue;
                    }
                    // == Progress bar ===
                    var lineBytes = Encoding.UTF8.GetByteCount(line) + NewLineBytes;
                    processedBytes += lineBytes;
                    var progress = Math.Min(100, (double)processedBytes / totalBytes * 100);
                    ProgressManager.UpdateProgress(taskId, progress, filename);
                    // == Progress bar ===

                    // Step1: Parse the log record
                    LogDataPoint? logDP = null;

                    logDP = logParser.parse(line, logFilter);


                    if (logDP != null)
                    {
                        // Step2: Group the log record
                        var logGrouperResult = logGrouper.Execute(logDP, _batchSize);

                        if (logGrouperResult != null)
                        {
                            var logRecordListToInsertToInfluxDB = logGrouperResult.PointDataFlushList;
                            var logGrouperInfo = logGrouperResult.LogDataPointFlushList;

                            // Step3: Flush the log record to InfluxDB
                            if (logRecordListToInsertToInfluxDB != null)
                            {
                                if (logDP != null && _settings.IsEnabled && logDP.Timestamp != null)
                                {
                                    for (int i = 0; i < logRecordListToInsertToInfluxDB.Count; i++)
                                    {
                                        //// Test: Exception problem, test tracking
                                        //if (currentLineCompleted == 5000) throw new Exception();

                                        await System.IO.File.AppendAllTextAsync(outputFilePath, logRecordListToInsertToInfluxDB[i].ToLineProtocol() + Environment.NewLine);

                                        currentLineCompleted += logGrouperInfo[i].GroupItemCumCount; // In case the exception occurs, then this count should reflect exact log record line being processed

                                        // === Update log profile ===
                                        _logProfile.OutputPath = outputFilePath;
                                        _logProfile.EndAt = DateTime.Now;
                                        _logProfile.LastLine = currentLineCompleted;
                                        _logProfile.ProgressPercent = progress;

                                        LogParsingTracker.GetInstance().UpdateLogProfileToReport(_logProfile);
                                        // === Update log profile ===
                                    }

                                    currentLineCompleted = currentLineRead;
                                }
                            }
                        }
                    }
                    else
                    {
                        // If no data point can be extracted, then set it as completed.
                        currentLineCompleted += 1;
                    }

                    currentLineRead += 1;
                }

                //var remainingLogRecordInFlushList = logGrouper.GetRemainDataPointsIfExists() ?? [];
                var remainLogGrouperResult = logGrouper.GetRemainLogGrouperResult();
                if (remainLogGrouperResult == null) throw new Exception("remainLogGrouperResult can't be null");
                var remainLogRecordListToInsertToInfluxDB = remainLogGrouperResult.PointDataFlushList;
                var remainLogGrouperInfo = remainLogGrouperResult.LogDataPointFlushList;

                for (int i = 0; i < remainLogRecordListToInsertToInfluxDB.Count; i++)
                {
                    await System.IO.File.AppendAllTextAsync(outputFilePath, remainLogRecordListToInsertToInfluxDB[i].ToLineProtocol() + Environment.NewLine);
                    currentLineCompleted += remainLogGrouperInfo[i].GroupItemCumCount; // In case the exception occurs, then this count should reflect exact log record line being processed

                    // === Update log profile ===
                    _logProfile.OutputPath = outputFilePath;
                    _logProfile.EndAt = DateTime.Now;
                    _logProfile.LastLine = currentLineCompleted;

                    LogParsingTracker.GetInstance().UpdateLogProfileToReport(_logProfile);
                    // === Update log profile ===
                }

                // === Update log profile ===
                _logProfile.ProgressPercent = 100;
                LogParsingTracker.GetInstance().UpdateLogProfileToReport(_logProfile);
                // === Update log profile ===


                ProgressManager.UpdateProgress(taskId, 100, filename);


            }
            catch (Exception ex)
            {
                _logProfile.Message = ex.Message;
            }
            finally
            {
                _logProfile.OutputPath = outputFilePath;
                _logProfile.EndAt = DateTime.Now;
                _logProfile.LastLine = currentLineCompleted;
                LogParsingTracker.GetInstance().UpdateLogProfileToReport(_logProfile);
            }

            return _logProfile;
        }

    }
}

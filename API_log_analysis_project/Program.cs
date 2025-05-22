using API_log_analysis_project;
using API_log_analysis_project.Entities;
using API_log_analysis_project.Factories;
using API_log_analysis_project.Parsers;
using API_log_analysis_project.Trackers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

internal class Program
{
    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    private delegate bool EventHandler(CtrlType sig);
    static EventHandler _handler;

    enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    private static bool Handler(CtrlType sig)
    {
        LogParsingTracker _tracker = LogParsingTracker.GetInstance();
        _tracker.SetMessageToReport("The process has been interrupted unexpectedly!");
        _tracker.StopParsingProcess();
        //switch (sig)
        //{
        //    case CtrlType.CTRL_C_EVENT:
        //    case CtrlType.CTRL_LOGOFF_EVENT:N
        //    case CtrlType.CTRL_SHUTDOWN_EVENT:
        //    case CtrlType.CTRL_CLOSE_EVENT:
        //    default:
        //        return false;
        //}

        return false;
    }

    private static async Task Main(string[] args)
    {

        _handler += new EventHandler(Handler);
        SetConsoleCtrlHandler(_handler, true);

        ConsoleInteraction ci = new ConsoleInteraction(LogParsingTracker.GetInstance());
        ci.InitializedGlobalRootDirPath();

        try
        {
            ci.GetLogParsingTracker().StartParsingProcess();

            while (true)
            {
                Console.WriteLine("Would you like to\n0: Process logs, \n1: Process selected logs \n2: Exit?");
                Console.Write("Your choice:");
                string userChoice = Console.ReadLine() ?? "";
                userChoice = userChoice.ToUpper();

                if (userChoice == "0") await ci.ProcessLogs();
                else if (userChoice == "1") await ci.ProcessSelectedLogs();
                else if (userChoice == "2")
                {
                    break;
                }
                else Console.WriteLine("[WARN] Can't recognize the symbol, please input again...\n");
            }

            ci.GetLogParsingTracker().SetMessageToReport("Success");
        }
        catch (Exception ex)
        {
            ci.GetLogParsingTracker().SetMessageToReport(ex.Message);
        }
        finally
        {
            ci.GetLogParsingTracker().StopParsingProcess();
        }
    }
}

//// Read JSON file
//string fileName = @"C:\Users\vinsonyip\Desktop\Projects\API_Log_Analysis_Project\LogParserStatusTrackingJson.json";
//string fileNameOutput = @"C:\Users\vinsonyip\Desktop\Projects\API_Log_Analysis_Project\LogParserStatusTrackingJson_test.json";
//LogParsingReport o1 = JsonSerializer.Deserialize<LogParsingReport>(File.ReadAllText(fileName));

//string jsonString = JsonSerializer.Serialize(o1);
//File.WriteAllText(fileNameOutput, jsonString);

//Console.WriteLine("");


//// Parse & write single log to DB
//ILogParser logParser = ParserFactory.GetParser(ParserName.P3_API_Parser);
//ILogFilter logFilter = new P3APILogFilter { IsCollectRequestLog = true, IsCollectResponseLog = false };
//ILogGrouper logGrouper = new P3APILogGrouper();
//string logFilePath = @"C:\Users\vinsonyip\Desktop\Projects\API_Log_Analysis_Project\gmobile-global-2025-02-28-33-extract.log";
//DBWriter dBWriter = new DBWriter();
//dBWriter.WriteLogToInfluxDB(logFilePath, logParser, logFilter, logGrouper);

////dBWriter.WriteLogToInfluxDB(logFilePath, logParser, logFilter);
////dBWriter.WriteLogToTxt(logFilePath, logParser, logFilter, logGrouper);

//string directoryPath = @"C:\Users\vinsonyip\Desktop\Projects\API_Log_Analysis_Project";
//if (Directory.Exists(directoryPath))
//{
//    string[] logFiles = Directory.GetFiles(directoryPath, "*.log", SearchOption.TopDirectoryOnly);
//    Console.WriteLine("");
//}

//string tmpLine = @"2025-02-28 16:13:14.703 [default task-15137] WARN: Invalid cookie header: ""Set-Cookie: AWSALBTG=IzPDqzE1T+PHx6LivCjrrgZ2Aa4IEaKqJOytSdukpkwhkeU+db/512xP0e1JXdWgrRkSI8KNEDeQmuUak39T0JZSDG8TpmiCsWVYNq2KxNkX2DQU8npQFQReaB/3y2tv0p7WxwM5pliyBoEMVcwZ2Qbn0MUMquJqo7k6zml0YFXW; Expires=Fri, 07 Mar 2025 08:13:14 GMT; Path=/"". Invalid 'expires' attribute: Fri, 07 Mar 2025 08:13:14 GMT";
//// Step1: Parse the log record
//string APIActionStr = new APIActionParser().parse(ref tmpLine);
//Console.WriteLine(APIActionStr);



//string log = @"2025-05-07 23:09:29.840 +08:00 [ERR] User:;Action:GetEmailAddress;Status:failure;";
//string pattern = @"(?<=Action:)[^;]+";

//Match match = Regex.Match(log, pattern);

//if (match.Success)
//{
//    Console.WriteLine(match.Value); // Direct output: "default task-15086"
//}

//string result = new MessageLevelParser().parse(ref log);
//string result1 = new APIActionParser().parse(ref log);

//var pdata = new P3SMSAPILogParser().parse(log);
//Console.WriteLine(pdata.ToString());
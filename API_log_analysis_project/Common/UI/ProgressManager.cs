using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_log_analysis_project.Common.UI
{
    public static class ProgressManager
    {
        private static readonly object _lock = new();
        private static readonly Dictionary<Guid, int> _taskLines = new();
        private static int _baseLine = -1;
        private static int _nextLine = 0;

        private const string FULL_BLOCK = "█";
        private const string EMPTY_BLOCK = "░";
        private static readonly bool _unicodeSupported;

        private static int _lastProgressBottom = 0;

        static ProgressManager()
        {
            try
            {
                // Set console encoding to UTF-8
                Console.OutputEncoding = Encoding.UTF8;

                // Test Unicode character display
                _unicodeSupported = !string.IsNullOrWhiteSpace(
                    new string('█', 1).Normalize(NormalizationForm.FormKD));
            }
            catch
            {
                _unicodeSupported = false;
            }
        }

        private static string BuildProgressBar(double percentage, string filename)
        {
            // Calculate available space
            const int fixedElementsLength = 15; // "File X: ", percentage, and padding
            int maxFilenameLength = 50;
            int maxBarWidth = Console.WindowWidth - fixedElementsLength - maxFilenameLength;

            // Ensure minimum bar width
            maxBarWidth = Math.Max(20, Math.Min(maxBarWidth, 100));

            // Build bar components
            int completeWidth = (int)(maxBarWidth * percentage / 100);
            string bar = $"{new string('█', completeWidth)}{new string('░', maxBarWidth - completeWidth)}";

            // Truncate filename with ellipsis
            string truncatedName = filename.Length > maxFilenameLength
                ? filename[..(maxFilenameLength - 3)] + "..."
                : filename.PadRight(maxFilenameLength);

            return $"[{bar}] {truncatedName} {percentage:0.0}%";
        }

        public static Guid RegisterTask()
        {
            lock (_lock)
            {
                var taskId = Guid.NewGuid();

                if (_baseLine == -1)
                {
                    _baseLine = Console.CursorTop;
                    Console.WriteLine(); // Reserve space for status line
                }

                int line = _nextLine++;
                _taskLines[taskId] = line;

                UpdateConsole(() =>
                {
                    Console.SetCursorPosition(0, _baseLine + line);
                    Console.Write($"File {line + 1}: [          ] 0.0%");
                });

                return taskId;
            }
        }

        public static void CompleteTask(Guid taskId)
        {
            lock (_lock)
            {
                if (_taskLines.TryGetValue(taskId, out int line))
                {
                    // Clear progress bar line
                    UpdateConsole(() =>
                    {
                        int safeTop = Math.Min(_baseLine + line, Console.BufferHeight - 1);
                        Console.SetCursorPosition(0, safeTop);
                        //Console.Write(new string(' ', Console.WindowWidth - 1));
                    });

                    // Remove from tracking
                    _taskLines.Remove(taskId);

                    // Compact progress bar space if last task
                    if (_taskLines.Count == 0)
                    {
                        // Remember where progress bars ended
                        _lastProgressBottom = _baseLine + _nextLine;

                        _baseLine = -1;
                        _nextLine = 0;

                        Console.SetCursorPosition(0, _lastProgressBottom);
                    }
                }
            }
        }

        public static void UpdateProgress(Guid taskId, double percentage, string filename)
        {
            lock (_lock)
            {
                if (!_taskLines.TryGetValue(taskId, out var line)) return;

                UpdateConsole(() =>
                {
                    var safeTop = Math.Min(_baseLine + line, Console.BufferHeight - 1);
                    Console.SetCursorPosition(0, safeTop);

                    string progressText = BuildProgressBar(percentage, filename);
                    Console.Write(progressText.PadRight(Console.WindowWidth - 1));
                });

                // Automatically handle completion
                if (percentage >= 100)
                {
                    CompleteTask(taskId);
                }
            }
        }

        public static void WriteBelowProgress(string message)
        {
            lock (_lock)
            {
                // Save current position
                var (left, top) = Console.GetCursorPosition();

                // Move below progress bars
                Console.SetCursorPosition(0, Math.Max(_baseLine + _nextLine, top));
                Console.WriteLine(message);

                // Restore original position
                Console.SetCursorPosition(left, top);
            }
        }

        //private static string BuildProgressBar(double percentage, int width)
        //{
        //    int complete = (int)(width * percentage / 100);
        //    return $"[{new string('█', complete)}{new string('░', width - complete)}]";
        //}

        private static void UpdateConsole(Action action)
        {
            var (originalLeft, originalTop) = Console.GetCursorPosition();
            try
            {
                action();
            }
            finally
            {
                Console.SetCursorPosition(
                    Math.Clamp(originalLeft, 0, Console.BufferWidth - 1),
                    Math.Clamp(originalTop, 0, Console.BufferHeight - 1)
                );
            }
        }
    }

    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }

}


using System;
using System.IO;
using System.Reflection.Emit;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Decryptor.Utils
{
    public static class Logger
    {
        private static ILogger _logger;
        private static string _defaultOutputTemplate =
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        // Colors for different levels
        private static string _infoColor = "\x1b[38;5;10m"; // Green
        private static string _debugColor = "\x1b[38;5;14m"; // Cyan
        private static string _resetColor = "\x1b[0m"; // Reset

        public static void Initialize(bool isDebugMode, string outputTemplate = null)
        {
            if (outputTemplate == null)
            {
                outputTemplate = _defaultOutputTemplate;
            }

            var level = isDebugMode ? LogEventLevel.Debug : LogEventLevel.Information;

            _logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console(new CustomFormatter(outputTemplate))
                .CreateLogger();

            Info($"Logger initialized with level: {level}");
        }

        public static void Info(string message, bool indent = false, string label = "")
        {
            if (indent)
            {
                message = "  " + message;
            }

            if (string.IsNullOrWhiteSpace(label))
            {
                label = "[+]";
            }

            _logger.Information($"{_infoColor}{label} {message}{_resetColor}");
        }

        public static void Debug(string message, bool indent = false, string label = "")
        {
            if (indent)
            {
                message = "  " + message;
            }

            if (string.IsNullOrWhiteSpace(label))
            {
                label = "[+]";
            }

            _logger.Debug($"{_debugColor}{label} {message}{_resetColor}");
        }

        private class CustomFormatter : ITextFormatter
        {
            private readonly MessageTemplateTextFormatter _defaultFormatter;

            public CustomFormatter(string outputTemplate)
            {
                _defaultFormatter = new MessageTemplateTextFormatter(outputTemplate);
            }

            public void Format(LogEvent logEvent, TextWriter output)
            {
                _defaultFormatter.Format(logEvent, output);
            }
        }
    }
}

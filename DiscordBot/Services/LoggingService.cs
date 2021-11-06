using Discord;
using System;
using System.Threading.Tasks;


namespace DiscordBot.Services
{
    public class LoggingService
    {
        public static async Task LogCriticalAsync(string source, string message, Exception exc = null)
            => await LogAsync(source, LogSeverity.Critical, message, exc);

        public static async Task LogInformationAsync(string source, string message)
            => await LogAsync(source, LogSeverity.Info, message);


        public static async Task LogAsync(string src, LogSeverity severity, string message, Exception exception = null)
        {
            if (severity.Equals(null))
                severity = LogSeverity.Warning;

            await Append($"{GetSeverityString(severity)}", GetConsoleColor(severity));
            await Append($" [{SourceToString(src)}] ", ConsoleColor.DarkGray);

            if (!string.IsNullOrWhiteSpace(message))
                await Append($"{message}\n", ConsoleColor.White);
            else if (exception.Message == null)
                await Append("Unknown Exeption. Exception returned Null.\n", ConsoleColor.DarkRed);
            else
                await Append($"{exception.Message ?? "Unknown"}\n{exception.StackTrace ?? "Unknown"}\n",
                    GetConsoleColor(severity));
        }

        private static async Task Append(string message, ConsoleColor color)
        {
            await Task.Run(() =>
            {
                Console.ForegroundColor = color;
                Console.Write(message);
            });
        }

        private static string SourceToString(string src)
        {
            switch (src.ToLower())
            {
                case "discord":
                    return "DISCD";
                case "victoria":
                    return "VICTR";
                case "audio":
                    return "AUDIO";
                case "admin":
                    return "ADMIN";
                case "gateway":
                    return "GTWAY";
                case "blacklist":
                    return "BLAKL";
                case "lavanode_0_socket":
                    return "LAVAS";
                case "lavanode_0":
                    return "LAVA#";
                case "bot":
                    return "BOTWN";
                default: return src;
            }
        }

        private static string GetSeverityString(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return "CRIT";
                case LogSeverity.Debug:
                    return "DBUG";
                case LogSeverity.Error:
                    return "EROR";
                case LogSeverity.Info:
                    return "INFO";
                case LogSeverity.Verbose:
                    return "VERB";
                case LogSeverity.Warning:
                    return "WARN";
                default: return "UNKN";
            }
        }

        private static ConsoleColor GetConsoleColor(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return ConsoleColor.Red;
                case LogSeverity.Debug:
                    return ConsoleColor.Magenta;
                case LogSeverity.Error:
                    return ConsoleColor.DarkRed;
                case LogSeverity.Info:
                    return ConsoleColor.Green;
                case LogSeverity.Verbose:
                    return ConsoleColor.DarkCyan;
                case LogSeverity.Warning:
                    return ConsoleColor.Yellow;
                default: return ConsoleColor.White;
            }
        }
    }
}
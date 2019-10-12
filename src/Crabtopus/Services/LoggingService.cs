using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Crabtopus.App.Services
{
    public class LoggingService
    {
        private readonly string _logDirectory;
        private readonly string _logFile;

        public LoggingService(DiscordSocketClient discord, CommandService commands)
        {
            _ = discord ?? throw new ArgumentNullException(nameof(discord));
            _ = commands ?? throw new ArgumentNullException(nameof(commands));

            _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            _logFile = Path.Combine(_logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.txt");

            discord.Log += OnLogAsync;
            commands.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            if (!File.Exists(_logFile))
            {
                File.Create(_logFile).Dispose();
            }

            string logText = $"{DateTime.UtcNow.ToString("hh:mm:ss", CultureInfo.InvariantCulture)} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(_logFile, $"{logText}{Environment.NewLine}");

            return Console.Out.WriteLineAsync(logText);
        }
    }
}

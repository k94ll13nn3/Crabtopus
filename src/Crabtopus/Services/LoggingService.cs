using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Crabtopus.App.Services
{
    public class LoggingService
    {
        public LoggingService(DiscordSocketClient discord, CommandService commands)
        {
            _ = discord ?? throw new ArgumentNullException(nameof(discord));
            _ = commands ?? throw new ArgumentNullException(nameof(commands));

            discord.Log += OnLogAsync;
            commands.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            return Console.Out.WriteLineAsync($"{DateTime.Now.ToShortTimeString()} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}");
        }
    }
}

using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace Crabtopus.App.Modules
{
    [Name("Info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;

        public InfoModule(IConfigurationRoot config)
        {
            _config = config;
        }

        [Command("nextset"), Alias("ns")]
        [Summary("Affiche le prochain set")]
        public async Task GetNextSetAsync()
        {
            var releaseDate = DateTime.ParseExact(_config["NextSetRelease"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            await ReplyAsync($"**{_config["NextSetName"]}** sort dans {(int)Math.Ceiling((releaseDate - DateTime.Now).TotalDays)} jours (normalement :thinking:).");
        }
    }
}

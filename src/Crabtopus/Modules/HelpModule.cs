using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Crabtopus.App.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;

        public HelpModule(CommandService service, IConfigurationRoot config, DiscordSocketClient discord)
        {
            _service = service;
            _config = config;
            _discord = discord;
        }

        [Command("help"), Alias("h")]
        [Summary("Affiche ce message")]
        public async Task HelpAsync()
        {
            string prefix = _config["Prefix"];
            var builder = new StringBuilder();
            builder.AppendLine($"Voici les commandes de {_discord.CurrentUser.Username} :");
            foreach (ModuleInfo module in _service.Modules)
            {
                foreach (CommandInfo cmd in module.Commands)
                {
                    PreconditionResult result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        string parameters = string.Empty;
                        if (cmd.Parameters.Count > 0)
                        {
                            parameters = $" *{string.Join("* *", cmd.Parameters.Select(p => p.Name))}*";
                        }

                        builder.AppendLine($"**{prefix}{cmd.Aliases[0]}{(cmd.Aliases.Count > 1 ? $" ({cmd.Aliases[1]})" : "")}**{parameters} : {cmd.Summary}");
                    }
                }
            }

            await ReplyAsync(builder.ToString());
        }
    }
}

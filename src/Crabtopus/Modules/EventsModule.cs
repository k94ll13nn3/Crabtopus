using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crabtopus.App.Model;
using Crabtopus.App.Services;
using Discord.Commands;
using Discord.Rest;

namespace Crabtopus.App.Modules
{
    [Name("Events")]
    public class EventsModule : ModuleBase<SocketCommandContext>
    {
        private readonly FetchService _fetchService;

        public EventsModule(FetchService fetchService)
        {
            _fetchService = fetchService;
        }

        [Command("events"), Alias("e")]
        [Summary("Affiche les derniers tournois standard.")]
        public async Task GetEventsAsync()
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync(Messages.Fetching);
            IEnumerable<EventInfo> eventInfos = await _fetchService.GetEventsAsync();
            var builder = new StringBuilder();
            if (eventInfos.Any())
            {
                builder.AppendLine("```");
                foreach (EventInfo eventInfo in eventInfos)
                {
                    builder.AppendLine($"{eventInfo.Id} {eventInfo.Date.ToShortDateString()} {new string('*', eventInfo.Rating).PadRight(3, '\u00A0')} {eventInfo.Name}");
                }
                builder.AppendLine("```");
            }
            else
            {
                builder.Append(Messages.NoData);
            }

            await msg.ModifyAsync(m => m.Content = Regex.Replace(builder.ToString(), "[ ]{2,}", " "));
        }

        [Command("majorevents"), Alias("me")]
        [Summary("Affiche les derniers tournois majeurs standard.")]
        public async Task GetMajorEventsAsync()
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync(Messages.Fetching);
            IEnumerable<EventInfo> eventInfos = await _fetchService.GetEventsAsync();
            var builder = new StringBuilder();
            if (eventInfos.Any())
            {
                builder.AppendLine("```");
                foreach (EventInfo eventInfo in eventInfos.Where(x => x.Rating == 3))
                {
                    builder.AppendLine($"{eventInfo.Id} {eventInfo.Date.ToShortDateString()} {new string('*', eventInfo.Rating).PadRight(3, ' ')} {eventInfo.Name}");
                }
                builder.AppendLine("```");
            }
            else
            {
                builder.Append(Messages.NoData);
            }

            await msg.ModifyAsync(m => m.Content = builder.ToString());
        }
    }
}

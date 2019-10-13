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
    [Name("Decks")]
    public class DecksModule : ModuleBase<SocketCommandContext>
    {
        private readonly FetchService _fetchService;

        public DecksModule(FetchService fetchService)
        {
            _fetchService = fetchService;
        }

        [Command("decks"), Alias("ed")]
        [Summary("Affiche les decks du tournoi spécifié.")]
        public async Task GetDecksAsync(int eventId)
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync(Messages.Fetching);
            await GetDecksInternalAsync(eventId, msg);
        }

        [Command("lastdecks"), Alias("led")]
        [Summary("Affiche les decks du dernier tournoi standard.")]
        public async Task GetDecksAsync()
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync(Messages.Fetching);
            (_, IEnumerable<EventData> events) = await _fetchService.GetEventsAsync();
            if (events.Any())
            {
                await GetDecksInternalAsync(events.First().Id, msg);
                return;
            }

            await msg.ModifyAsync(m => m.Content = Messages.NoData);
        }

        private async Task GetDecksInternalAsync(int eventId, RestUserMessage msg)
        {
            (string address, IEnumerable<EventDeck> eventDecks) = await _fetchService.GetDecksAsync(eventId);
            var builder = new StringBuilder();
            if (eventDecks.Any())
            {
                builder.AppendLine(address);
                builder.AppendLine("```");
                int placementSize = eventDecks.Max(x => x.Placement.Length);
                foreach (EventDeck eventDeck in eventDecks)
                {
                    builder.AppendLine($"{eventDeck.Id} {eventDeck.Placement.PadLeft(placementSize, '\u00A0')} {eventDeck.Name} ({eventDeck.User})");
                }
                builder.AppendLine("```");
            }
            else
            {
                builder.Append(Messages.NoData);
            }

            await msg.ModifyAsync(m => m.Content = Regex.Replace(builder.ToString(), "[ ]{2,}", " "));
        }
    }
}

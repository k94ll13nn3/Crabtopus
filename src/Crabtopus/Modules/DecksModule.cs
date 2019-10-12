using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crabtopus.Core;
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
            IEnumerable<EventDeck> eventDecks = await _fetchService.GetDecksAsync(eventId);
            var builder = new StringBuilder();
            if (eventDecks.Any())
            {
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

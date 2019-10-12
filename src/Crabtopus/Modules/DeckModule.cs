using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crabtopus.Core.Model;
using Crabtopus.Core.Services;
using Discord.Commands;
using Discord.Rest;

namespace Crabtopus.App.Modules
{
    [Name("Deck")]
    public class DeckModule : ModuleBase<SocketCommandContext>
    {
        private readonly FetchService _fetchService;

        public DeckModule(FetchService fetchService)
        {
            _fetchService = fetchService;
        }

        [Command("deck"), Alias("d")]
        [Summary("Affiche le decks spécifié.")]
        public async Task GetDecksAsync(int eventId, int deckId)
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync(Messages.Fetching);
            Deck deck = await _fetchService.GetDeckAsync(eventId, deckId);
            var builder = new StringBuilder();
            if (deck != null)
            {
                builder.AppendLine("```");
                foreach (Card card in deck.Maindeck)
                {
                    builder.AppendLine($"{card.Count} {card.Name}");
                }

                builder.AppendLine("");
                foreach (Card card in deck.Sideboard)
                {
                    builder.AppendLine($"{card.Count} {card.Name}");
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

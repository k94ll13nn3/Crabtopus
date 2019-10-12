using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crabtopus.App.Model;
using Crabtopus.App.Services;
using Discord.Commands;
using Discord.Rest;

namespace Crabtopus.App.Modules
{
    [Name("Deck")]
    public class DeckModule : ModuleBase<SocketCommandContext>
    {
        private readonly FetchService _fetchService;
        private readonly CardsService _cardsService;

        public DeckModule(FetchService fetchService, CardsService cardsService)
        {
            _fetchService = fetchService;
            _cardsService = cardsService;
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
                    CardData cardData = _cardsService.Cards.Find(x => x.Title == card.Name && x.CollectorNumber != "0");
                    builder.AppendLine($"{card.Count} {card.Name} ({cardData.Set}) {cardData.CollectorNumber}");
                }

                builder.AppendLine("");
                foreach (Card card in deck.Sideboard)
                {
                    CardData cardData = _cardsService.Cards.Find(x => x.Title == card.Name && x.CollectorNumber != "0");
                    builder.AppendLine($"{card.Count} {card.Name} ({cardData.Set}) {cardData.CollectorNumber}");
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

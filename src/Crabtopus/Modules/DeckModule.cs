using System;
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
        [Summary("Affiche le deck spécifié.")]
        public async Task GetDeckAsync(int eventId, int deckId)
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync(Messages.Fetching);
            await GetDeckInternalAsync(eventId, deckId, msg);
        }

        [Command("top1deck"), Alias("td")]
        [Summary("Affiche le deck top1 du tournoi spécifié.")]
        public async Task GetDeckAsync(int eventId)
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync(Messages.Fetching);
            (_, IEnumerable<EventDeck> decks) = await _fetchService.GetDecksAsync(eventId);
            if (decks.Any())
            {
                await GetDeckInternalAsync(eventId, decks.First().Id, msg);
                return;
            }

            await msg.ModifyAsync(m => m.Content = Messages.NoData);
        }

        [Command("lasttop1deck"), Alias("ltd")]
        [Summary("Affiche le deck top1 du dernier tournoi standard.")]
        public async Task GetDeckAsync()
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync(Messages.Fetching);
            (_, IEnumerable<EventData> events) = await _fetchService.GetEventsAsync();
            if (events.Any())
            {
                (_, IEnumerable<EventDeck> decks) = await _fetchService.GetDecksAsync(events.First().Id);
                if (decks.Any())
                {
                    await GetDeckInternalAsync(events.First().Id, decks.First().Id, msg);
                    return;
                }
            }

            await msg.ModifyAsync(m => m.Content = Messages.NoData);
        }

        private async Task GetDeckInternalAsync(int eventId, int deckId, RestUserMessage msg)
        {
            (string address, Deck deck) = await _fetchService.GetDeckAsync(eventId, deckId);
            var builder = new StringBuilder();
            var list = new StringBuilder();
            var colors = new HashSet<CardColor>();
            var wildcards = new Wildcards();
            if (deck != null)
            {
                list.AppendLine("```");
                foreach (Card card in deck.Maindeck)
                {
                    string name = card.Name.Replace("/", "//", StringComparison.Ordinal);
                    CardData cardData = _cardsService.Cards.Find(x => x.Title == name && x.CollectorNumber != "0");
                    list.AppendLine($"{card.Count} {name} ({cardData.Set}) {cardData.CollectorNumber}");

                    wildcards.Add(card.Count, cardData.Rarity);
                    foreach (CardColor color in cardData.Colors)
                    {
                        colors.Add(color);
                    }
                }

                list.AppendLine("");
                foreach (Card card in deck.Sideboard)
                {
                    CardData cardData = _cardsService.Cards.Find(x => x.Title == card.Name && x.CollectorNumber != "0");
                    list.AppendLine($"{card.Count} {card.Name} ({cardData.Set}) {cardData.CollectorNumber}");

                    wildcards.Add(card.Count, cardData.Rarity);
                    foreach (CardColor color in cardData.Colors)
                    {
                        colors.Add(color);
                    }
                }

                list.AppendLine("```");

                builder.AppendLine(address);
                builder.AppendLine($"**{deck.Name}** {string.Concat(colors.OrderBy(x => x).Select(Emotes.Convert))}");
                builder.AppendLine(wildcards.ToString());
                builder.AppendLine(list.ToString());
            }
            else
            {
                builder.Append(Messages.NoData);
            }

            await msg.ModifyAsync(m => m.Content = Regex.Replace(builder.ToString(), "[ ]{2,}", " "));
        }
    }
}

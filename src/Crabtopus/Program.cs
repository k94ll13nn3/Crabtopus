using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;

namespace Crabtopus
{
    public static class Program
    {
        private const string BaseUrl = "www.mtgtop8.com";

        private static IBrowsingContext Context;

        public static int Main(string[] args)
        {
            Context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var rootCommand = new RootCommand("command")
            {
                Description = "A mtg top 8 deck fetcher!"
            };

            var eventsCommand = new Command("events")
            {
                Description = "Get recent events.",
                Handler = CommandHandler.Create(async () =>
                {
                    foreach (Event @event in await GetEventsAsync())
                    {
                        Console.WriteLine($"{@event.Id} {@event.Name} {@event.Date.ToShortDateString()} {new string('\u2605', @event.Rating)}");
                    }
                })
            };

            var decksCommand = new Command("decks")
            {
                Description = "Get decks for an event.",
                Handler = CommandHandler.Create<int>(async (eventId) =>
                {
                    foreach (EventDeck eventDeck in await GetDecksAsync(eventId))
                    {
                        Console.WriteLine($"{eventDeck.Id} {eventDeck.Name} {eventDeck.User} {eventDeck.Placement}");
                    }
                })
            };

            decksCommand.Add(new Argument<int>("event-id") { Description = "The id of the event." });

            var deckCommand = new Command("deck")
            {
                Description = "Get decks for an event.",
                Handler = CommandHandler.Create<int, int>(async (eventId, deckId) =>
                {
                    Deck deck = await GetDeckAsync(eventId, deckId);
                    Console.WriteLine(deck.Name);
                    Console.WriteLine("----------------------");
                    foreach (Card card in deck.Maindeck)
                    {
                        Console.WriteLine($"{card.Count} {card.Name}");
                    }

                    Console.WriteLine();
                    foreach (Card card in deck.Sideboard)
                    {
                        Console.WriteLine($"{card.Count} {card.Name}");
                    }
                })
            };

            deckCommand.Add(new Argument<int>("event-id") { Description = "The id of the deck." });
            deckCommand.Add(new Argument<int>("deck-id") { Description = "The id of the event." });

            rootCommand.Add(eventsCommand);
            rootCommand.Add(decksCommand);
            rootCommand.Add(deckCommand);

            return rootCommand.InvokeAsync(args).Result;
        }

        private static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            string address = $"https://{BaseUrl}/format?f=ST";
            IDocument document = await Context.OpenAsync(address);
            const string eventsSelector =
                ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(3) > tbody:nth-child(1) > tr:not(:first-child)," +
                ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:not(:first-child)";
            IHtmlCollection<IElement> eventsCells = document.QuerySelectorAll(eventsSelector);
            var events = new List<Event>();
            foreach (IElement cell in eventsCells)
            {
                IElement link = cell.QuerySelector("td:nth-child(1) > a");
                int id = int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{link.GetAttribute("href")}").Query)["e"], CultureInfo.InvariantCulture);
                string name = link.TextContent;
                int rating = cell.QuerySelector("td:nth-child(2)").ChildElementCount;
                DateTime date = DateTime.ParseExact(cell.QuerySelector("td:nth-child(3)").TextContent, "dd/MM/yy", CultureInfo.CurrentCulture);
                if (!events.Select(x => x.Id).Contains(id))
                {
                    events.Add(new Event(id, name, date, rating));
                }
            }

            return events.OrderByDescending(x => x.Date);
        }

        private static async Task<IEnumerable<EventDeck>> GetDecksAsync(int eventId)
        {
            string address = $"https://{BaseUrl}/event?e={eventId}&f=ST";
            IDocument document = await Context.OpenAsync(address);
            const string cellSelector = "div.chosen_tr, div.hover_tr";
            IHtmlCollection<IElement> cells = document.QuerySelectorAll(cellSelector);
            var decks = new List<EventDeck>();
            foreach (IElement cell in cells.Take(8))
            {
                string placement = cell.QuerySelector("div:nth-child(1)").TextContent;
                IElement link = cell.QuerySelector("div:nth-child(2) > a");
                int id = int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{link.GetAttribute("href")}").Query)["d"], CultureInfo.InvariantCulture);
                string name = link.TextContent;
                string user = cell.QuerySelector("div:nth-child(3)").TextContent;
                decks.Add(new EventDeck(id, name, user, placement));
            }

            return decks;
        }

        private static async Task<Deck> GetDeckAsync(int eventId, int deckId)
        {
            string address = $"https://{BaseUrl}/event?e={eventId}&d={deckId}&f=ST";
            IDocument document = await Context.OpenAsync(address);
            const string mainDeckSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:not(:last-child) > table > tbody > tr > td > div:nth-child(1)";
            const string sideboardSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:last-child > table > tbody > tr > td > div:nth-child(1)";
            IHtmlCollection<IElement> mainDeckCells = document.QuerySelectorAll(mainDeckSelector);
            IHtmlCollection<IElement> sideboardCells = document.QuerySelectorAll(sideboardSelector);

            string name = document.QuerySelector("div.chosen_tr").QuerySelector("div:nth-child(2) > a").TextContent;
            IEnumerable<Card> maindeck = mainDeckCells.Select(m => new Card((int)char.GetNumericValue(m.TextContent[0]), m.TextContent[1..].Trim()));
            IEnumerable<Card> sideboard = sideboardCells.Select(m => new Card((int)char.GetNumericValue(m.TextContent[0]), m.TextContent[1..].Trim()));

            return new Deck(name, maindeck, sideboard);
        }
    }
}

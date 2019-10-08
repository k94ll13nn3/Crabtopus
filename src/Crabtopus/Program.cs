using System;
using System.Collections.Generic;
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

        public static async Task Main()
        {
            Context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            IEnumerable<Event> events = await GetEventsAsync();
            foreach (Event @event in events)
            {
                Console.WriteLine($"{@event.Id} {@event.Name} {@event.Date} {@event.Rating}");
                IEnumerable<EventDeck> decks = await GetDecksAsync(@event.Id);
                foreach (EventDeck deck in decks)
                {
                    Console.WriteLine($"{deck.Id} {deck.Name} {deck.User} {deck.Placement}");
                }
            }
        }

        private static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            string address = $"https://{BaseUrl}/format?f=ST";
            IDocument document = await Context.OpenAsync(address);
            const string eventsSelector = ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(3) > tbody:nth-child(1) > tr:not(:first-child)";
            IHtmlCollection<IElement> eventsCells = document.QuerySelectorAll(eventsSelector);
            var events = new List<Event>();
            foreach (IElement cell in eventsCells)
            {
                IElement link = cell.QuerySelector("td:nth-child(1) > a");
                int id = int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{link.GetAttribute("href")}").Query)["e"], CultureInfo.InvariantCulture);
                string name = link.TextContent;
                int rating = cell.QuerySelector("td:nth-child(2)").ChildElementCount;
                string date = cell.QuerySelector("td:nth-child(3)").TextContent;
                events.Add(new Event(id, name, date, rating));
            }

            return events;
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

        private static async Task<IEnumerable<(int count, string title, bool sideboard)>> GetDeckAsync(int eventId, int deckId)
        {
            string address = $"https://{BaseUrl}/event?e={eventId}&d={deckId}&f=ST";
            IDocument document = await Context.OpenAsync(address);
            const string mainDeckSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:not(:last-child) > table > tbody > tr > td > div:nth-child(1)";
            const string sideboardSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:last-child > table > tbody > tr > td > div:nth-child(1)";
            IHtmlCollection<IElement> mainDeckCells = document.QuerySelectorAll(mainDeckSelector);
            IHtmlCollection<IElement> sideboardCells = document.QuerySelectorAll(sideboardSelector);

            const string nameSelector = ".S16";
            string nameCells = document.QuerySelectorAll(nameSelector).Select(m => m.TextContent).First();

            Console.WriteLine(nameCells);

            return mainDeckCells.Select(m => (count: (int)char.GetNumericValue(m.TextContent[0]), title: m.TextContent[1..].Trim(), sideboard: false))
                .Concat(sideboardCells.Select(m => (count: (int)char.GetNumericValue(m.TextContent[0]), title: m.TextContent[1..].Trim(), sideboard: true)));
        }
    }
}

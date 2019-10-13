using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using Crabtopus.App.Model;

namespace Crabtopus.App.Services
{
    public class FetchService
    {
        private const string BaseUrl = "www.mtgtop8.com";

        private readonly IBrowsingContext _context;

        public FetchService()
        {
            _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        }

        public async Task<(string address, IEnumerable<EventData> data)> GetEventsAsync()
        {
            string address = $"https://{BaseUrl}/format?f=ST";
            IDocument document = await _context.OpenAsync(address);
            const string eventsSelector =
                ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(3) > tbody:nth-child(1) > tr:not(:first-child)," +
                ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:not(:first-child)";
            IHtmlCollection<IElement> eventsCells = document.QuerySelectorAll(eventsSelector);
            var events = new List<EventData>();
            foreach (IElement cell in eventsCells)
            {
                IElement link = cell.QuerySelector("td:nth-child(1) > a");
                int id = int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{link.GetAttribute("href")}").Query)["e"], CultureInfo.InvariantCulture);
                string name = link.TextContent;
                int rating = cell.QuerySelector("td:nth-child(2)").ChildElementCount;
                DateTime date = DateTime.ParseExact(cell.QuerySelector("td:nth-child(3)").TextContent, "dd/MM/yy", CultureInfo.CurrentCulture);
                if (!events.Select(x => x.Id).Contains(id))
                {
                    events.Add(new EventData(id, name, date, rating));
                }
            }

            return (address, events.OrderByDescending(x => x.Date));
        }

        public async Task<(string address, IEnumerable<EventDeck> data)> GetDecksAsync(int eventId)
        {
            string address = $"https://{BaseUrl}/event?e={eventId}&f=ST";
            IDocument document = await _context.OpenAsync(address);
            const string cellSelector = "div.chosen_tr, div.hover_tr";
            IHtmlCollection<IElement> cells = document.QuerySelectorAll(cellSelector);
            var decks = new List<EventDeck>();
            foreach (IElement cell in cells.Take(8))
            {
                IElement link = cell.QuerySelector("div:nth-child(2) > a");
                if (link != null)
                {
                    string placement = cell.QuerySelector("div:nth-child(1)")?.TextContent;
                    int id = int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{link.GetAttribute("href")}").Query)["d"], CultureInfo.InvariantCulture);
                    string name = link.TextContent;
                    string user = cell.QuerySelector("div:nth-child(3)").TextContent;
                    decks.Add(new EventDeck(id, name, user, placement));
                }
            }

            return (address, decks);
        }

        public async Task<(string address, Deck data)> GetDeckAsync(int eventId, int deckId)
        {
            string address = $"https://{BaseUrl}/event?e={eventId}&d={deckId}&f=ST";
            IDocument document = await _context.OpenAsync(address);
            const string mainDeckSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:not(:last-child) > table > tbody > tr > td > div:nth-child(1)";
            const string sideboardSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:last-child > table > tbody > tr > td > div:nth-child(1)";
            IHtmlCollection<IElement> mainDeckCells = document.QuerySelectorAll(mainDeckSelector);
            IHtmlCollection<IElement> sideboardCells = document.QuerySelectorAll(sideboardSelector);

            if (mainDeckCells.Any())
            {
                string name = document.QuerySelector("div.chosen_tr").QuerySelector("div:nth-child(2) > a").TextContent;
                IEnumerable<Card> maindeck = mainDeckCells.Select(m => new Card((int)char.GetNumericValue(m.TextContent[0]), m.TextContent[1..].Trim()));
                IEnumerable<Card> sideboard = sideboardCells.Select(m => new Card((int)char.GetNumericValue(m.TextContent[0]), m.TextContent[1..].Trim()));

                return (address, new Deck(name, maindeck, sideboard));
            }
            else
            {
                return (null, null);
            }
        }
    }
}

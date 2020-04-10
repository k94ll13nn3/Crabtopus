using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using Crabtopus.Models;

namespace Crabtopus.Services
{
    internal class FetchService : IFetchService
    {
        private const string BaseUrl = "www.mtgtop8.com";

        private readonly IBrowsingContext _context;

        public FetchService()
        {
            _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        }

        public async Task<IEnumerable<Tournament>> GetEventsAsync()
        {
            string address = $"https://{BaseUrl}/format?f=ST";
            IDocument document = await _context.OpenAsync(address);
            const string eventsSelector =
                ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(3) > tbody:nth-child(1) > tr:not(:first-child)," +
                ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:not(:first-child)";
            IHtmlCollection<IElement> eventsCells = document.QuerySelectorAll(eventsSelector);
            var events = new List<Tournament>();
            foreach (IElement cell in eventsCells)
            {
                IElement link = cell.QuerySelector("td:nth-child(1) > a");
                int id = int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{link.GetAttribute("href")}").Query)["e"], CultureInfo.InvariantCulture);
                string name = link.TextContent;
                int rating = cell.QuerySelector("td:nth-child(2)").ChildElementCount;
                if (cell.QuerySelector("td:nth-child(2)").FirstElementChild.GetAttribute("src") == "graph/bigstar.png")
                {
                    rating = 4;
                }

                DateTime date = DateTime.ParseExact(cell.QuerySelector("td:nth-child(3)").TextContent, "dd/MM/yy", CultureInfo.CurrentCulture);
                if (!events.Select(x => x.Id).Contains(id))
                {
                    IEnumerable<Deck> decks = await GetDecksAsync(id);
                    events.Add(new Tournament(id, name, date, rating, decks));
                    break; //TODO
                }
            }

            return events.OrderByDescending(x => x.Date);
        }

        private async Task<IEnumerable<Deck>> GetDecksAsync(int eventId)
        {
            string address = $"https://{BaseUrl}/event?e={eventId}&f=ST";
            IDocument document = await _context.OpenAsync(address);
            const string cellSelector = "div.chosen_tr, div.hover_tr";
            IHtmlCollection<IElement> cells = document.QuerySelectorAll(cellSelector);
            var decks = new List<Deck>();
            foreach (IElement cell in cells.Take(8))
            {
                IElement link = cell.QuerySelector("div:nth-child(2) > a");
                if (link != null)
                {
                    string placement = cell.QuerySelector("div:nth-child(1)")?.TextContent ?? string.Empty;
                    int id = int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{link.GetAttribute("href")}").Query)["d"], CultureInfo.InvariantCulture);
                    string name = link.TextContent;
                    string user = cell.QuerySelector("div:nth-child(3)").TextContent;
                    (IEnumerable<DeckCard> maindeck, IEnumerable<DeckCard> sideboard) = await GetDeckAsync(eventId, id);
                    decks.Add(new Deck(id, name, user, placement, maindeck, sideboard));
                    break; //TODO
                }
            }

            return decks;
        }

        private async Task<(IEnumerable<DeckCard> maindeck, IEnumerable<DeckCard> sideboard)> GetDeckAsync(int eventId, int deckId)
        {
            string address = $"https://{BaseUrl}/event?e={eventId}&d={deckId}&f=ST";
            IDocument document = await _context.OpenAsync(address);
            const string mainDeckSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:not(:last-child) > table > tbody > tr > td > div:nth-child(1)";
            const string sideboardSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:last-child > table > tbody > tr > td > div:nth-child(1)";
            IHtmlCollection<IElement> mainDeckCells = document.QuerySelectorAll(mainDeckSelector);
            IHtmlCollection<IElement> sideboardCells = document.QuerySelectorAll(sideboardSelector);

            if (mainDeckCells.Any())
            {
                IEnumerable<DeckCard> maindeck = mainDeckCells.Select(m => new DeckCard((int)char.GetNumericValue(m.TextContent[0]), m.TextContent[1..].Trim()));
                IEnumerable<DeckCard> sideboard = sideboardCells.Select(m => new DeckCard((int)char.GetNumericValue(m.TextContent[0]), m.TextContent[1..].Trim()));

                return (maindeck, sideboard);
            }
            else
            {
                return (new List<DeckCard>(), new List<DeckCard>());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;

namespace Crabtopus
{
    public static class Program
    {
        private const string _baseUrl = "www.mtgtop8.com";

        private static IBrowsingContext _context;

        public static async Task Main()
        {
            bool sideboardLine = false;
            _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var events = await GetEvents();
            var decks = await GetDecks(events.First().id);
            foreach (var (count, title, sideboard) in await GetDeck(events.First().id, decks.First().id))
            {
                if (sideboard && !sideboardLine)
                {
                    Console.WriteLine();
                    sideboardLine = true;
                }

                Console.WriteLine($"{count} {title}");
            }
        }

        private static async Task<IEnumerable<(string title, int id)>> GetEvents()
        {
            string address = $"https://{_baseUrl}/format?f=ST";
            var document = await _context.OpenAsync(address);
            const string cellSelector = ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(3) > tbody:nth-child(1) > tr > td:nth-child(1) > a:nth-child(1)";
            var cells = document.QuerySelectorAll(cellSelector);
            return cells.Select(m => (title: m.TextContent, id: int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{_baseUrl}/{m.GetAttribute("href")}").Query)["e"], CultureInfo.InvariantCulture)));
        }

        private static async Task<IEnumerable<(string title, int id)>> GetDecks(int eventId)
        {
            string address = $"https://{_baseUrl}/event?e={eventId}&f=ST";
            var document = await _context.OpenAsync(address);
            const string cellSelector = "div.chosen_tr > div:nth-child(2) > a:nth-child(1), div.hover_tr > div:nth-child(2) > a:nth-child(1)";
            var cells = document.QuerySelectorAll(cellSelector);
            return cells.Select(m => (title: m.TextContent, id: int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{_baseUrl}/{m.GetAttribute("href")}").Query)["d"], CultureInfo.InvariantCulture)));
        }

        private static async Task<IEnumerable<(int count, string title, bool sideboard)>> GetDeck(int eventId, int deckId)
        {
            string address = $"https://{_baseUrl}/event?e={eventId}&d={deckId}&f=ST";
            var document = await _context.OpenAsync(address);
            const string mainDeckSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:not(:last-child) > table > tbody > tr > td > div:nth-child(1)";
            const string sideboardSelector = "table.Stable:nth-child(3) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:last-child > table > tbody > tr > td > div:nth-child(1)";
            var mainDeckCells = document.QuerySelectorAll(mainDeckSelector);
            var sideboardCells = document.QuerySelectorAll(sideboardSelector);

            const string nameSelector = ".S16";
            var nameCells = document.QuerySelectorAll(nameSelector).Select(m => m.TextContent).First();

            System.Console.WriteLine(nameCells);

            return mainDeckCells.Select(m => (count: (int)char.GetNumericValue(m.TextContent[0]), title: m.TextContent[1..].Trim(), sideboard: false))
                .Concat(sideboardCells.Select(m => (count: (int)char.GetNumericValue(m.TextContent[0]), title: m.TextContent[1..].Trim(), sideboard: true)));
        }
    }
}

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
            bool sideboardLine = false;
            Context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            IEnumerable<(string title, int id)> events = await GetEventsAsync();
            foreach ((string title, int id) in events)
            {
                Console.WriteLine($"{id} {title}");
            }

            Console.WriteLine("---------------------------------------");
            IEnumerable<(string title, int id)> decks = await GetDecksAsync(events.First().id);
            foreach ((string title, int id) in decks)
            {
                Console.WriteLine($"{id} {title}");
            }

            Console.WriteLine("---------------------------------------");
            foreach ((int count, string title, bool sideboard) in await GetDeckAsync(events.First().id, decks.First().id))
            {
                if (sideboard && !sideboardLine)
                {
                    Console.WriteLine();
                    sideboardLine = true;
                }

                Console.WriteLine($"{count} {title}");
            }
        }

        private static async Task<IEnumerable<(string title, int id)>> GetEventsAsync()
        {
            string address = $"https://{BaseUrl}/format?f=ST";
            IDocument document = await Context.OpenAsync(address);
            const string cellSelector = ".page > div:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(3) > tbody:nth-child(1) > tr > td:nth-child(1) > a:nth-child(1)";
            IHtmlCollection<IElement> cells = document.QuerySelectorAll(cellSelector);
            return cells.Select(m => (title: m.TextContent, id: int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{m.GetAttribute("href")}").Query)["e"], CultureInfo.InvariantCulture)));
        }

        private static async Task<IEnumerable<(string title, int id)>> GetDecksAsync(int eventId)
        {
            string address = $"https://{BaseUrl}/event?e={eventId}&f=ST";
            IDocument document = await Context.OpenAsync(address);
            const string cellSelector = "div.chosen_tr > div:nth-child(2) > a:nth-child(1), div.hover_tr > div:nth-child(2) > a:nth-child(1)";
            IHtmlCollection<IElement> cells = document.QuerySelectorAll(cellSelector);
            return cells.Select(m => (title: m.TextContent, id: int.Parse(HttpUtility.ParseQueryString(new Uri($"https://{BaseUrl}/{m.GetAttribute("href")}").Query)["d"], CultureInfo.InvariantCulture)));
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

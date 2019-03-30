using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Crabtopus
{
    public static class Program
    {
        public static async Task Main()
        {
            var logReader = new LogReader();
            if (!logReader.ReadLog())
            {
                return;
            }

            Console.WriteLine($"Version: {logReader.Version}");

            var services = new ServiceCollection();
            services.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IHttpClientFactory httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            var cardManager = new CardManager(logReader.Version, logReader.Endpoint, httpClientFactory);
            await cardManager.InitializeCardsAsync();
            List<Card> cards = cardManager.Cards;

            Blob blob = logReader.Blobs.First(x => x.Method == "GetDeckListsV3");
            Deck[] decks = JsonConvert.DeserializeObject<Deck[]>(blob.Content);
            List<long> cardsInDeck = decks[0].MainDeck;

            Console.WriteLine(decks[0].Name);
            for (int i = 0; i < cardsInDeck.Count; i += 2)
            {
                Card card = cards.Find(x => x.Id == cardsInDeck[i]);
                if (card is null)
                {
                    Console.WriteLine($"Unknown card: {cardsInDeck[i]}");
                }
                else
                {
                    Console.WriteLine($"{cardsInDeck[i + 1]} {card.Title} ({card.Set}) {card.CollectorNumber}");
                }
            }
        }
    }
}

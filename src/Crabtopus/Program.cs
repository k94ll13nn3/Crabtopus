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
            logReader.ReadLog();

            var services = new ServiceCollection();
            services.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IHttpClientFactory httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            var cardManager = new CardManager(logReader.Version, logReader.Endpoint, httpClientFactory);
            await cardManager.InitializeCardsAsync();
            List<Card> cards = cardManager.Cards;

            Blob deckListsBlob = logReader.Blobs.First(x => x.Method == "GetDeckListsV3");
            Deck[] decks = JsonConvert.DeserializeObject<Deck[]>(deckListsBlob.Content);
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

            Console.WriteLine();
            Console.WriteLine();
            Blob playerCardsBlob = logReader.Blobs.First(x => x.Method == "GetPlayerCardsV3");
            Dictionary<int, int> collection = JsonConvert.DeserializeObject<Dictionary<int, int>>(playerCardsBlob.Content);
            Console.WriteLine($"Total cards in collection: {collection.Sum(x => x.Value)}");
            foreach (KeyValuePair<int, int> cardInfo in collection)
            {
                Card card = cards.Find(x => x.Id == cardInfo.Key);
                if (card is null)
                {
                    Console.WriteLine($"Unknown card: {cardInfo.Key}");
                }
                else
                {
                    Console.WriteLine($"{cardInfo.Value} {card.Title} ({card.Set}) {card.CollectorNumber}");
                }
            }
        }
    }
}

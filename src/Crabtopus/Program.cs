using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Crabtopus
{
    public static class Program
    {
        public static void Main()
        {
            string content = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\Wizards Of The Coast\MTGA\output_log.txt");
            List<Blob> blobs = new BlobReader(content).GetBlobs();
            foreach (IGrouping<string, Blob> item in blobs.GroupBy(x => x.Type))
            {
                Console.WriteLine(item.Key);
                foreach (Blob item2 in item)
                {
                    Console.WriteLine($"\t{item2.Method}({item2.Id})");
                }
            }

            // Sample deck
            // TODO: handle mythic edition (ex.: Teferi)
            // TODO: handle transform cards name (ex.: Search for Azcanta)
            Blob blob = blobs.First(x => x.Method == "GetDeckListsV3");
            Deck[] decks = JsonConvert.DeserializeObject<Deck[]>(blob.Content);
            List<long> deck = decks[0].MainDeck;

            ScryfallCard[] scryfallCards = JsonConvert.DeserializeObject<ScryfallCard[]>(File.ReadAllText("scryfall.arena.json"));
            for (int i = 0; i < deck.Count; i += 2)
            {
                ScryfallCard card = scryfallCards.FirstOrDefault(x => x.ArenaId == deck[i]);
                if (card is null)
                {
                    Console.WriteLine($"Unknown card: {deck[i]}");
                }
                else
                {
                    Console.WriteLine($"{deck[i + 1]} {card.Name} ({(card.Set == "dom" ? "DAR" : card.Set.ToUpper())}) {card.CollectorNumber}");
                }
            }
        }
    }
}

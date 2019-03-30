using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Crabtopus
{
    public static class Program
    {
        public static async Task Main()
        {
            string content = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\Wizards Of The Coast\MTGA\output_log.txt");
            var logReader = new LogReader(content);
            logReader.ReadLog();

            var services = new ServiceCollection();
            services.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IHttpClientFactory httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            HttpClient mtgarenaClient = httpClientFactory.CreateClient("mtgarena");

            string hash = await mtgarenaClient.GetStringAsync(logReader.Endpoint);
            byte[] manifestCompressed = await mtgarenaClient.GetByteArrayAsync($"Manifest_{hash}.mtga");
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(Unzip(manifestCompressed));
            string cardsFileName = manifest.Assets.First(x => x.Name.StartsWith("data_cards_")).Name;
            string locFileName = manifest.Assets.First(x => x.Name.StartsWith("data_loc_")).Name;
            byte[] cardsCompressed = await mtgarenaClient.GetByteArrayAsync(cardsFileName);
            byte[] locCompressed = await mtgarenaClient.GetByteArrayAsync(locFileName);
            List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(Unzip(cardsCompressed));
            Loc loc = JsonConvert.DeserializeObject<List<Loc>>(Unzip(locCompressed)).Find(x => x.Langkey == "EN");

            Blob blob = logReader.Blobs.First(x => x.Method == "GetDeckListsV3");
            Deck[] decks = JsonConvert.DeserializeObject<Deck[]>(blob.Content);
            List<long> cardsInDeck = decks[0].MainDeck;

            Console.WriteLine(decks[0].Name);
            for (int i = 0; i < cardsInDeck.Count; i += 2)
            {
                Card card = cards.Find(x => x.Grpid == cardsInDeck[i]);
                if (card is null)
                {
                    Console.WriteLine($"Unknown card: {cardsInDeck[i]}");
                }
                else
                {
                    string text = loc.Keys.Find(x => x.Id == card.TitleId)?.Text;
                    Console.WriteLine($"{cardsInDeck[i + 1]} {text} ({card.Set}) {card.CollectorNumber}");
                }
            }
        }

        public static string Unzip(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            using (var outputStream = new MemoryStream())
            using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                byte[] bytes = new byte[4096];
                int count;
                while ((count = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    outputStream.Write(bytes, 0, count);
                }

                return Encoding.UTF8.GetString(outputStream.ToArray());
            }
        }
    }

    public class Loc
    {
        public string Langkey { get; set; }
        public string IsoCode { get; set; }
        public List<Key> Keys { get; set; }
    }

    public class Key
    {
        public long Id { get; set; }
        public string Text { get; set; }
    }
}

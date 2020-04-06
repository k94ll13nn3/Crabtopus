using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Crabtopus.Model;
using Newtonsoft.Json;

namespace Crabtopus
{
    public class CardManager
    {
        private readonly string _version;
        private readonly string _endpoint;
        private readonly HttpClient _mtgarenaClient;

        public CardManager(LogReader logReader, IHttpClientFactory httpClientFactory)
        {
            _mtgarenaClient = httpClientFactory.CreateClient("mtgarena");
            _version = logReader.Version;
            _endpoint = logReader.Endpoint;
        }

        public List<Card> Cards { get; set; }

        public async Task LoadCardsAsync()
        {
            if (!Directory.Exists(_version))
            {
                Directory.CreateDirectory(_version);
            }

            try
            {
                string hash = await _mtgarenaClient.GetStringAsync(_endpoint);
                byte[] compressedManifest = await _mtgarenaClient.GetByteArrayAsync($"Manifest_{hash}.mtga");
                string uncompressedManifest = Unzip(compressedManifest);
                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(uncompressedManifest);
                Asset cardsAsset = manifest.Assets.First(x => x.Name.StartsWith("data_cards_"));
                Asset localizationsAsset = manifest.Assets.First(x => x.Name.StartsWith("data_loc_"));

                string cardsHash = null;
                string localizationHash = null;
                string cardsHashPath = Path.Combine(_version, "cards.hash");
                string localizationHashPath = Path.Combine(_version, "localization.hash");
                if (File.Exists(cardsHashPath))
                {
                    cardsHash = File.ReadAllText(cardsHashPath);
                }

                if (File.Exists(localizationHashPath))
                {
                    localizationHash = File.ReadAllText(localizationHashPath);
                }

                string cardsPath = Path.Combine(_version, "cards.json");
                if (File.Exists(cardsPath) && cardsHash == cardsAsset.Hash && localizationHash == localizationsAsset.Hash)
                {
                    Cards = JsonConvert.DeserializeObject<List<Card>>(File.ReadAllText(cardsPath));
                }
                else
                {
                    File.WriteAllText(cardsHashPath, cardsAsset.Hash);
                    File.WriteAllText(localizationHashPath, localizationsAsset.Hash);

                    string cardsFileName = cardsAsset.Name;
                    byte[] compressedCards = await _mtgarenaClient.GetByteArrayAsync(cardsFileName);
                    string uncompressedCards = Unzip(compressedCards);
                    List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(uncompressedCards);

                    string localizationsFileName = localizationsAsset.Name;
                    byte[] compressedLocalizations = await _mtgarenaClient.GetByteArrayAsync(localizationsFileName);
                    string uncompressedLocalizations = Unzip(compressedLocalizations);
                    Localization englishLocalization = JsonConvert
                        .DeserializeObject<List<Localization>>(uncompressedLocalizations)
                        .Find(x => x.IsoCode == "en-US");

                    foreach (Card card in cards)
                    {
                        card.Title = englishLocalization.Keys.Find(x => x.Id == card.TitleId)?.Text;
                    }

                    Cards = cards;
                    File.WriteAllText(cardsPath, JsonConvert.SerializeObject(Cards));
                }
            }
            catch (HttpRequestException e)
            {
                string cardsPath = Path.Combine(_version, "cards.json");
                if (File.Exists(cardsPath))
                {
                    Cards = JsonConvert.DeserializeObject<List<Card>>(File.ReadAllText(cardsPath));
                }
                else
                {
                    throw new InvalidOperationException("Cannot read cards.", e);
                }
            }
        }

        private static string Unzip(byte[] data)
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
}

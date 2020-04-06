using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Crabtopus.Models;

namespace Crabtopus
{
    internal class CardManager
    {
        private readonly string _version;
        private readonly string _endpoint;
        private readonly HttpClient _mtgarenaClient;
        private List<Card> _cards = new List<Card>();

        public CardManager(LogReader logReader, IHttpClientFactory httpClientFactory)
        {
            _mtgarenaClient = httpClientFactory.CreateClient("mtgarena");
            _version = logReader.Version;
            _endpoint = logReader.Endpoint;
        }

        public Card Get(string set, string collectorNumber)
        {
            return _cards.First(x => x.Set == set && x.CollectorNumber == collectorNumber);
        }

        public Card GetById(string id)
        {
            return _cards.First(x => $"{x.Id}" == id);
        }

        public async Task LoadCardsAsync()
        {
            if (!Directory.Exists(_version))
            {
                Directory.CreateDirectory(_version);
            }

            try
            {
                string hash = await _mtgarenaClient.GetStringAsync(new Uri(_endpoint, UriKind.Relative));
                byte[] compressedManifest = await _mtgarenaClient.GetByteArrayAsync(new Uri($"Manifest_{hash}.mtga", UriKind.Relative));
                string uncompressedManifest = Unzip(compressedManifest);
                Manifest manifest = JsonSerializer.Deserialize<Manifest>(uncompressedManifest);
                Asset cardsAsset = manifest.Assets.First(x => x.Name.StartsWith("data_cards_", StringComparison.OrdinalIgnoreCase));
                Asset localizationsAsset = manifest.Assets.First(x => x.Name.StartsWith("data_loc_", StringComparison.OrdinalIgnoreCase));

                string cardsHash = string.Empty;
                string localizationHash = string.Empty;
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
                    _cards = JsonSerializer.Deserialize<List<Card>>(File.ReadAllText(cardsPath));
                }
                else
                {
                    File.WriteAllText(cardsHashPath, cardsAsset.Hash);
                    File.WriteAllText(localizationHashPath, localizationsAsset.Hash);

                    string cardsFileName = cardsAsset.Name;
                    byte[] compressedCards = await _mtgarenaClient.GetByteArrayAsync(new Uri(cardsFileName, UriKind.Relative));
                    string uncompressedCards = Unzip(compressedCards);
                    List<Card> cards = JsonSerializer.Deserialize<List<Card>>(uncompressedCards);

                    string localizationsFileName = localizationsAsset.Name;
                    byte[] compressedLocalizations = await _mtgarenaClient.GetByteArrayAsync(new Uri(localizationsFileName, UriKind.Relative));
                    string uncompressedLocalizations = Unzip(compressedLocalizations);
                    Localization englishLocalization = JsonSerializer
                        .Deserialize<List<Localization>>(uncompressedLocalizations)
                        .First(x => x.IsoCode == "en-US");

                    foreach (Card card in cards)
                    {
                        card.Title = englishLocalization.Keys.First(x => x.Id == card.TitleId).Text;
                    }

                    _cards = cards;
                    File.WriteAllText(cardsPath, JsonSerializer.Serialize(_cards));
                }
            }
            catch (HttpRequestException e)
            {
                string cardsPath = Path.Combine(_version, "cards.json");
                if (File.Exists(cardsPath))
                {
                    _cards = JsonSerializer.Deserialize<List<Card>>(File.ReadAllText(cardsPath));
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

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
using Microsoft.Extensions.Options;

namespace Crabtopus.Data
{
    internal class CardReader
    {
        private readonly string _version;
        private readonly string _endpoint;
        private readonly HttpClient _mtgarenaClient;
        private readonly Database _database;

        public CardReader(IOptions<ApplicationSettings> options, IHttpClientFactory httpClientFactory, Database database)
        {
            _mtgarenaClient = httpClientFactory.CreateClient("mtgarena");
            _version = options.Value.Version;
            _endpoint = options.Value.Endpoint;
            _database = database;
        }

        public async Task LoadCardsAsync()
        {
            GameInfo gameVersion = _database.Set<GameInfo>().FindOne(_ => true) ?? new GameInfo();

            // Cards are loaded only if the version in base if different from the current game version.
            if (gameVersion.Version != _version)
            {
                try
                {
                    string hash = await _mtgarenaClient.GetStringAsync(new Uri(_endpoint, UriKind.Relative));
                    byte[] compressedManifest = await _mtgarenaClient.GetByteArrayAsync(new Uri($"Manifest_{hash}.mtga", UriKind.Relative));
                    string uncompressedManifest = Unzip(compressedManifest);
                    Manifest manifest = JsonSerializer.Deserialize<Manifest>(uncompressedManifest);
                    Asset cardsAsset = manifest.Assets.First(x => x.Name.StartsWith("data_cards_", StringComparison.OrdinalIgnoreCase));
                    Asset localizationsAsset = manifest.Assets.First(x => x.Name.StartsWith("data_loc_", StringComparison.OrdinalIgnoreCase));

                    string cardsHash = gameVersion.CardsHash;
                    string localizationHash = gameVersion.LocalizationHash;

                    if (cardsHash != cardsAsset.Hash || localizationHash != localizationsAsset.Hash)
                    {
                        gameVersion.Version = _version;
                        gameVersion.CardsHash = cardsAsset.Hash;
                        gameVersion.LocalizationHash = localizationsAsset.Hash;

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

                        _database.Set<GameInfo>().Upsert(gameVersion);
                        _database.Set<Card>().DeleteAll();
                        _database.Set<Card>().InsertBulk(cards);
                    }
                }
                catch (HttpRequestException e)
                {
                    throw new InvalidOperationException("Cannot load cards.", e);
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

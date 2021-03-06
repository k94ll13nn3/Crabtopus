﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Crabtopus.Models;
using Crabtopus.Models.Json;
using Microsoft.Extensions.Options;
using EFCore.BulkExtensions;

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
            GameInfo gameVersion = _database.GameInfos.OrderByDescending(x => x.Version).FirstOrDefault() ?? new GameInfo();

            // Cards are loaded only if the version in base if different from the current game version.
            if (gameVersion.Version != _version)
            {
                try
                {
                    string hash = (await _mtgarenaClient.GetStringAsync(new Uri(_endpoint, UriKind.Relative)).ConfigureAwait(false)).Split(new[] { '\n', '\r' })[0];
                    byte[] compressedManifest = await _mtgarenaClient.GetByteArrayAsync(new Uri($"Manifest_{hash}.mtga", UriKind.Relative)).ConfigureAwait(false);
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
                        byte[] compressedCards = await _mtgarenaClient.GetByteArrayAsync(new Uri(cardsFileName, UriKind.Relative)).ConfigureAwait(false);
                        string uncompressedCards = Unzip(compressedCards);
                        List<CardInfo> deserializedCards = JsonSerializer.Deserialize<List<CardInfo>>(uncompressedCards);

                        string localizationsFileName = localizationsAsset.Name;
                        byte[] compressedLocalizations = await _mtgarenaClient.GetByteArrayAsync(new Uri(localizationsFileName, UriKind.Relative)).ConfigureAwait(false);
                        string uncompressedLocalizations = Unzip(compressedLocalizations);
                        Localization englishLocalization = JsonSerializer
                            .Deserialize<List<Localization>>(uncompressedLocalizations)
                            .First(x => x.IsoCode == "en-US");

                        foreach (CardInfo card in deserializedCards)
                        {
                            card.Name = englishLocalization.Keys.First(x => x.Id == card.TitleId).Text;
                        }

                        var cards = deserializedCards.Where(x => !x.IsToken && x.IsPrimaryCard).Select(x => new Card
                        {
                            CollectorNumber = x.CollectorNumber,
                            Id = x.Id,
                            Set = x.Set,
                            Name = x.Name,
                            Rarity = x.RarityValue switch
                            {
                                2 => Rarity.Common,
                                3 => Rarity.Uncommon,
                                4 => Rarity.Rare,
                                5 => Rarity.MythicRare,
                                _ => Rarity.BasicLand,
                            },
                            Colors = string.Join(';', x.Colors),
                            Types = string.Join(';', x.Types),
                            Cost = GetCost(x, deserializedCards),
                            ConvertedManaCost = x.ConvertedManaCost,
                        }).ToList();

                        gameVersion.Id = 0;
                        _database.GameInfos.Add(gameVersion);
                        _database.BulkInsertOrUpdate(cards);
                        _database.SaveChanges();
                    }
                }
                catch (HttpRequestException e)
                {
                    throw new InvalidOperationException("Cannot load cards.", e);
                }
            }
        }

        private static string GetCost(CardInfo card, List<CardInfo> deserializedCards)
        {
            // Land.
            if (string.Join(';', card.Types) == "5")
            {
                return string.Empty;
            }
            // Split card.
            else if (card.LinkedFaceType == 6)
            {
                return string.Join("o//", card.LinkedFaces.Select(linkedCardId => deserializedCards.Single(c => c.Id == linkedCardId).Cost));
            }

            return card.Cost;
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

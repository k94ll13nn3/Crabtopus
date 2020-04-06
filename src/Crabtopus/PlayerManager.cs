using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Crabtopus.Models;

namespace Crabtopus
{
    internal class PlayerManager
    {
        private static readonly Regex CardLineRegex = new Regex(@"(\d{1,2}) (.*?) \((\w+)\) (\w+)", RegexOptions.Compiled);
        private readonly CardManager _cardManager;
        private readonly Dictionary<Card, int> _collection;
        private readonly Wildcards _inventory;
        private readonly CombinedRankInfo _combinedRankInfo;

        public PlayerManager(CardManager cardManager, LogReader logReader)
        {
            _cardManager = cardManager;
            _collection = LoadCollection(logReader.Blobs.First(x => x.Method == "GetPlayerCardsV3"));
            _inventory = LoadInventory(logReader.Blobs.First(x => x.Method == "GetPlayerInventory"));
            _combinedRankInfo = LoadCombinedRankInfo(logReader.Blobs.First(x => x.Method == "GetCombinedRankInfo"));
        }

        public void DisplaySeasonStatistics()
        {
            Debug.WriteLine("Player season statistics:");
            Debug.WriteLine($"Rank: {_combinedRankInfo.ConstructedClass} {_combinedRankInfo.ConstructedLevel}");
            Debug.WriteLine($"Matches won: {_combinedRankInfo.ConstructedMatchesWon}");
            Debug.WriteLine($"Matches drawn: {_combinedRankInfo.ConstructedMatchesDrawn}");
            Debug.WriteLine($"Matches lost: {_combinedRankInfo.ConstructedMatchesLost}");
            Debug.WriteLine($"Total matches: {_combinedRankInfo.ConstructedMatchesTotal} ({_combinedRankInfo.ConstructedMatchesWon * 100.0 / _combinedRankInfo.ConstructedMatchesTotal:0.00}% W/L)");
            Debug.WriteLine("");
        }

        public void ValidateDeck(IEnumerable<string> deckList)
        {
            Deck newDeck = ParseDeckList(deckList);
            var missingCards = new List<DeckCard>();
            var validatedDeck = new Deck();
            foreach (DeckCard deckCard in newDeck.MainDeck)
            {
                Card card = _cardManager.Cards.First(x => x.Set == deckCard.Set && x.CollectorNumber == deckCard.CollectorNumber);

                deckCard.Rarity = card.Rarity;
                if (card.Rarity == Rarity.BasicLand)
                {
                    validatedDeck.MainDeck.Add(deckCard);
                    continue;
                }

                if (!_collection.ContainsKey(card) || _collection[card] < deckCard.Count)
                {
                    if (_collection.ContainsKey(card) && _collection[card] < deckCard.Count)
                    {
                        validatedDeck.MainDeck.Add(new DeckCard
                        {
                            CollectorNumber = deckCard.CollectorNumber,
                            Set = deckCard.Set,
                            Count = _collection[card],
                            Rarity = deckCard.Rarity,
                        });

                        IEnumerable<KeyValuePair<Card, int>> otherCards = _collection.Where(x => x.Key.Title == card.Title && x.Key != card);
                        int needed = deckCard.Count - _collection[card];
                        foreach (KeyValuePair<Card, int> otherCard in otherCards)
                        {
                            if (otherCard.Value >= needed)
                            {
                                validatedDeck.MainDeck.Add(new DeckCard
                                {
                                    CollectorNumber = otherCard.Key.CollectorNumber,
                                    Set = otherCard.Key.Set,
                                    Count = needed,
                                    Rarity = otherCard.Key.Rarity,
                                });

                                needed = 0;
                                break;
                            }
                            else
                            {
                                validatedDeck.MainDeck.Add(new DeckCard
                                {
                                    CollectorNumber = otherCard.Key.CollectorNumber,
                                    Set = otherCard.Key.Set,
                                    Count = otherCard.Value,
                                    Rarity = otherCard.Key.Rarity,
                                });

                                needed -= otherCard.Value;
                            }
                        }

                        if (needed > 0)
                        {
                            missingCards.Add(new DeckCard
                            {
                                CollectorNumber = deckCard.CollectorNumber,
                                Set = deckCard.Set,
                                Count = needed,
                                Rarity = deckCard.Rarity,
                            });
                        }
                    }
                    else
                    {
                        IEnumerable<KeyValuePair<Card, int>> otherCard = _collection.Where(x => x.Key.Title == card.Title);
                        if (otherCard.Sum(x => x.Value) < deckCard.Count)
                        {
                            missingCards.Add(deckCard);
                        }
                    }
                }
                else
                {
                    validatedDeck.MainDeck.Add(deckCard);
                }
            }

            var result = new ValidationResult(validatedDeck, missingCards, new Wildcards());

            foreach (IGrouping<Rarity, DeckCard> cardsByRarity in missingCards.GroupBy(x => x.Rarity))
            {
                int value = cardsByRarity.Sum(x => x.Count);
                switch (cardsByRarity.Key)
                {
                    case Rarity.Common:
                        result.Wildcards.Common = value;
                        break;

                    case Rarity.Uncommon:
                        result.Wildcards.Uncommon = value;
                        break;

                    case Rarity.Rare:
                        result.Wildcards.Rare = value;
                        break;

                    case Rarity.MythicRare:
                        result.Wildcards.MythicRare = value;
                        break;
                }
            }

            Debug.WriteLine("Deck:");
            foreach (DeckCard item in result.ValidatedDeck.MainDeck)
            {
                Card card = _cardManager.Cards.First(x => x.Set == item.Set && x.CollectorNumber == item.CollectorNumber);
                Debug.WriteLine($"{item.Count} {card.Title} ({card.Set}) {card.CollectorNumber}");
            }

            Debug.WriteLine("");
            Debug.WriteLine("Missing:");

            foreach (DeckCard item in result.MissingCards)
            {
                Card card = _cardManager.Cards.First(x => x.Set == item.Set && x.CollectorNumber == item.CollectorNumber);
                Debug.WriteLine($"{item.Count} {card.Title} ({card.Set}) {card.CollectorNumber}");
            }

            Debug.WriteLine("");

            Debug.WriteLine("Wildcards needed:");
            Debug.WriteLine($"Common: {result.Wildcards.Common} ({_inventory.Common})");
            Debug.WriteLine($"Uncommon: {result.Wildcards.Uncommon} ({_inventory.Uncommon})");
            Debug.WriteLine($"Rare: {result.Wildcards.Rare} ({_inventory.Rare})");
            Debug.WriteLine($"MythicRare: {result.Wildcards.MythicRare} ({_inventory.MythicRare})");
            Debug.WriteLine("");
        }

        private static Deck ParseDeckList(IEnumerable<string> deckList)
        {
            var deck = new Deck
            {
                MainDeck = new List<DeckCard>(),
                Sideboard = new List<DeckCard>(),
            };

            foreach (string line in deckList)
            {
                Match match = CardLineRegex.Match(line);
                if (match.Success)
                {
                    string collectorNumber = match.Groups[4].Value;
                    string set = match.Groups[3].Value;
                    DeckCard sameCardInDeck = deck.MainDeck.FirstOrDefault(x => x.Set == set && x.CollectorNumber == collectorNumber);
                    if (sameCardInDeck is null)
                    {
                        var card = new DeckCard { CollectorNumber = collectorNumber, Set = set };
                        card.Count = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                        deck.MainDeck.Add(card);
                    }
                    else
                    {
                        sameCardInDeck.Count += int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    }
                }
            }

            return deck;
        }

        private Dictionary<Card, int> LoadCollection(Blob collectionBlob)
        {
            var collection = new Dictionary<Card, int>();
            foreach (KeyValuePair<string, int> cardInfo in JsonSerializer.Deserialize<Dictionary<string, int>>(collectionBlob.Content))
            {
                Card card = _cardManager.Cards.First(x => $"{x.Id}" == cardInfo.Key);
                collection.Add(card, cardInfo.Value);
            }

            return collection;
        }

        private Wildcards LoadInventory(Blob inventoryBlob)
        {
            return JsonSerializer.Deserialize<Wildcards>(inventoryBlob.Content);
        }

        private CombinedRankInfo LoadCombinedRankInfo(Blob inventoryBlob)
        {
            return JsonSerializer.Deserialize<CombinedRankInfo>(inventoryBlob.Content);
        }
    }
}

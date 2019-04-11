using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crabtopus.Model;
using Newtonsoft.Json;

namespace Crabtopus
{
    public class PlayerManager
    {
        private static readonly Regex _cardLineRegex = new Regex(@"(\d{1,2}) (.*?) \((\w+)\) (\w+)", RegexOptions.Compiled);
        private readonly CardManager _cardManager;
        private readonly Dictionary<Card, int> _collection;
        private Wildcards _inventory;
        private CombinedRankInfo _combinedRankInfo;

        public PlayerManager(CardManager cardManager, LogReader logReader)
        {
            _cardManager = cardManager;
            _collection = new Dictionary<Card, int>();

            LoadCollection(logReader.Blobs.First(x => x.Method == "GetPlayerCardsV3"));
            LoadInventory(logReader.Blobs.First(x => x.Method == "GetPlayerInventory"));
            LoadCombinedRankInfo(logReader.Blobs.First(x => x.Method == "GetCombinedRankInfo"));
        }

        public void DisplaySeasonStatistics()
        {
            Console.WriteLine("Player season statistics:");
            Console.WriteLine($"Rank: {_combinedRankInfo.ConstructedClass} {_combinedRankInfo.ConstructedLevel}");
            Console.WriteLine($"Matches won: {_combinedRankInfo.ConstructedMatchesWon}");
            Console.WriteLine($"Matches drawn: {_combinedRankInfo.ConstructedMatchesDrawn}");
            Console.WriteLine($"Matches lost: {_combinedRankInfo.ConstructedMatchesLost}");
            Console.WriteLine($"Total matches: {_combinedRankInfo.ConstructedMatchesTotal} ({_combinedRankInfo.ConstructedMatchesWon * 100.0 / _combinedRankInfo.ConstructedMatchesTotal:0.00}% W/L)");
            Console.WriteLine();
            Console.WriteLine();
        }

        public ValidationResult ValidateDeck(IEnumerable<string> deckList)
        {
            Deck newDeck = ParseDeckList(deckList);
            var missingCards = new List<DeckCard>();
            var validatedDeck = new Deck();
            foreach (DeckCard deckCard in newDeck.MainDeck)
            {
                Card card = _cardManager.Cards.Find(x => x.Set == deckCard.Set && x.CollectorNumber == deckCard.CollectorNumber);
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

            var result = new ValidationResult
            {
                Wildcards = new Wildcards(),
                ValidatedDeck = validatedDeck,
                MissingCards = missingCards,
            };

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

            int c = result.ValidatedDeck.MainDeck.Sum(x => x.Count);
            int c2 = result.MissingCards.Sum(x => x.Count);

            foreach (DeckCard item in result.ValidatedDeck.MainDeck)
            {
                Card card = _cardManager.Cards.Find(x => x.Set == item.Set && x.CollectorNumber == item.CollectorNumber);
                Console.WriteLine($"{item.Count} {card.Title} ({card.Set}) {card.CollectorNumber}");
            }
            foreach (DeckCard item in result.MissingCards)
            {
                Card card = _cardManager.Cards.Find(x => x.Set == item.Set && x.CollectorNumber == item.CollectorNumber);
                Console.WriteLine($"{item.Count} {card.Title} ({card.Set}) {card.CollectorNumber}");
            }

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Wildcards needed:");
            Console.WriteLine($"Common: {result.Wildcards.Common} ({_inventory.Common})");
            Console.WriteLine($"Uncommon: {result.Wildcards.Uncommon} ({_inventory.Uncommon})");
            Console.WriteLine($"Rare: {result.Wildcards.Rare} ({_inventory.Rare})");
            Console.WriteLine($"MythicRare: {result.Wildcards.MythicRare} ({_inventory.MythicRare})");
            Console.WriteLine();
            Console.WriteLine();

            return result;
        }

        private void LoadCollection(Blob collectionBlob)
        {
            foreach (KeyValuePair<int, int> cardInfo in JsonConvert.DeserializeObject<Dictionary<int, int>>(collectionBlob.Content))
            {
                Card card = _cardManager.Cards.Find(x => x.Id == cardInfo.Key);
                if (card != null)
                {
                    _collection.Add(card, cardInfo.Value);
                }
            }
        }

        private void LoadInventory(Blob inventoryBlob)
        {
            _inventory = JsonConvert.DeserializeObject<Wildcards>(inventoryBlob.Content);
        }

        private void LoadCombinedRankInfo(Blob inventoryBlob)
        {
            _combinedRankInfo = JsonConvert.DeserializeObject<CombinedRankInfo>(inventoryBlob.Content);
        }

        private Deck ParseDeckList(IEnumerable<string> deckList)
        {
            var deck = new Deck
            {
                MainDeck = new List<DeckCard>(),
                Sideboard = new List<DeckCard>(),
            };

            foreach (string line in deckList)
            {
                Match match = _cardLineRegex.Match(line);
                if (match.Success)
                {
                    string collectorNumber = match.Groups[4].Value;
                    string set = match.Groups[3].Value;
                    DeckCard sameCardInDeck = deck.MainDeck.FirstOrDefault(x => x.Set == set && x.CollectorNumber == collectorNumber);
                    if (sameCardInDeck is null)
                    {
                        var card = new DeckCard { CollectorNumber = collectorNumber, Set = set };
                        card.Count = int.Parse(match.Groups[1].Value);
                        deck.MainDeck.Add(card);
                    }
                    else
                    {
                        sameCardInDeck.Count += int.Parse(match.Groups[1].Value);
                    }
                }
            }

            return deck;
        }
    }
}

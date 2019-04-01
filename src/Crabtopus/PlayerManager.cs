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
        private Inventory _inventory;

        public PlayerManager(CardManager cardManager)
        {
            _cardManager = cardManager;
            _collection = new Dictionary<Card, int>();
        }

        public void LoadCollection(Blob collectionBlob)
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

        public void LoadInventory(Blob inventoryBlob)
        {
            _inventory = JsonConvert.DeserializeObject<Inventory>(inventoryBlob.Content);
        }

        public bool CanCreateDeck(IEnumerable<string> deckList)
        {
            Deck newDeck = ParseDeckList(deckList);
            var missingCards = new Dictionary<Card, int>();
            foreach (DeckCard deckCard in newDeck.MainDeck)
            {
                Card card = _cardManager.Cards.Find(x => x.Set == deckCard.Set && x.CollectorNumber == deckCard.CollectorNumber);
                if (card.Rarity == Rarity.BasicLand)
                {
                    continue;
                }

                if (!_collection.ContainsKey(card) || _collection[card] < deckCard.Count)
                {
                    if (_collection.ContainsKey(card) && _collection[card] < deckCard.Count)
                    {
                        IEnumerable<KeyValuePair<Card, int>> otherCard = _collection.Where(x => x.Key.Title == card.Title && x.Key != card);
                        if (otherCard.Sum(x => x.Value) < deckCard.Count - _collection[card])
                        {
                            missingCards.Add(card, deckCard.Count - _collection[card]);
                        }
                    }
                    else
                    {
                        IEnumerable<KeyValuePair<Card, int>> otherCard = _collection.Where(x => x.Key.Title == card.Title);
                        if (otherCard.Sum(x => x.Value) < deckCard.Count)
                        {
                            missingCards.Add(card, deckCard.Count);
                        }
                    }
                }
            }

            foreach (IGrouping<Rarity, KeyValuePair<Card, int>> x in missingCards.GroupBy(x => x.Key.Rarity))
            {
                long wildcardForRarity = 0;
                switch (x.Key)
                {
                    case Rarity.Common:
                        wildcardForRarity = _inventory.CommonWildcards;
                        break;

                    case Rarity.Uncommon:
                        wildcardForRarity = _inventory.UncommonWildcards;
                        break;

                    case Rarity.Rare:
                        wildcardForRarity = _inventory.RareWildcards;
                        break;

                    case Rarity.MythicRare:
                        wildcardForRarity = _inventory.MythicRareWildcards;
                        break;
                }
                Console.WriteLine($"{x.Key}: {x.Sum(z => z.Value)} ({wildcardForRarity})");
            }

            return missingCards.Count > 0;
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

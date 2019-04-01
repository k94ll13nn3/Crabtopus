using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crabtopus.Model;
using Newtonsoft.Json;

namespace Crabtopus
{
    public class Player
    {
        private static readonly Regex _cardLineRegex = new Regex(@"(\d{1,2}) (.*?) \((\w+)\) (\w+)", RegexOptions.Compiled);
        private readonly CardManager _cardManager;
        private readonly Dictionary<Card, int> _collection;
        private Inventory _inventory;

        public Player(CardManager cardManager)
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
            foreach (KeyValuePair<Card, int> card in newDeck.MainDeck)
            {
                Card card2 = _cardManager.Cards.Find(x => x == card.Key);
                if (card2.Rarity == 1)
                {
                    continue;
                }

                if (!_collection.ContainsKey(card.Key) || _collection[card.Key] < card.Value)
                {
                    if (_collection.ContainsKey(card.Key) && _collection[card.Key] < card.Value)
                    {
                        IEnumerable<KeyValuePair<Card, int>> otherCard = _collection.Where(x => x.Key.Title == card.Key.Title && x.Key != card.Key);
                        if (otherCard.Sum(x => x.Value) < card.Value - _collection[card.Key])
                        {
                            missingCards.Add(card2, card.Value - _collection[card.Key]);
                        }
                    }
                    else
                    {
                        IEnumerable<KeyValuePair<Card, int>> otherCard = _collection.Where(x => x.Key.Title == card.Key.Title);
                        if (otherCard.Sum(x => x.Value) < card.Value)
                        {
                            missingCards.Add(card2, card.Value);
                        }
                    }
                }
            }

            foreach (IGrouping<int, KeyValuePair<Card, int>> x in missingCards.GroupBy(x => x.Key.Rarity))
            {
                long wildcardForRarity = 0;
                switch (x.Key)
                {
                    case 2:
                        wildcardForRarity = _inventory.CommonWildcards;
                        break;

                    case 3:
                        wildcardForRarity = _inventory.UncommonWildcards;
                        break;

                    case 4:
                        wildcardForRarity = _inventory.RareWildcards;
                        break;

                    case 5:
                        wildcardForRarity = _inventory.MythicWildcards;
                        break;
                }
                Console.WriteLine($"{x.Key}: {x.Sum(z => z.Value)} ({wildcardForRarity})");
            }

            return false;
        }

        private Deck ParseDeckList(IEnumerable<string> deckList)
        {
            var deck = new Deck
            {
                MainDeck = new Dictionary<Card, int>()
            };

            foreach (string line in deckList)
            {
                Match match = _cardLineRegex.Match(line);
                if (match.Success)
                {
                    var card = new Card { Title = match.Groups[2].Value, Set = match.Groups[3].Value, CollectorNumber = match.Groups[4].Value };
                    if (deck.MainDeck.ContainsKey(card))
                    {
                        deck.MainDeck[card] += int.Parse(match.Groups[1].Value);
                    }
                    else
                    {
                        deck.MainDeck[card] = int.Parse(match.Groups[1].Value);
                    }
                }
            }

            return deck;
        }
    }
}

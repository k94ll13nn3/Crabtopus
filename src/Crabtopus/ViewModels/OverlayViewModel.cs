using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Crabtopus.Models;
using Crabtopus.Services;

namespace Crabtopus.ViewModels
{
    public class TmpTournament
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<TmpDeck> Decks { get; } = new List<TmpDeck>();
    }

    public class TmpDeck
    {
        public string Name { get; set; } = string.Empty;
    }

    internal class OverlayViewModel : ViewModelBase
    {
        private static readonly Regex CardLineRegex = new Regex(@"(\d{1,2}) (.*?) \((\w+)\) (\w+)", RegexOptions.Compiled);
        private readonly ICardsService _cardsService;
        private readonly Dictionary<Card, int> _collection;
        private readonly Wildcards _inventory;
        private readonly CombinedRankInfo _combinedRankInfo;
        private string _title = "CRABTOPUS";
        private string _text = string.Empty;
        private bool _displayPopup;

        public OverlayViewModel(IBlobsService blobsService, ICardsService cardsService)
        {
            ShowPopupCommand = new DelegateCommand(() => DisplayPopup = true);
            ClosePopupCommand = new DelegateCommand(() => DisplayPopup = false);

            _cardsService = cardsService;
            _collection = LoadCollection(blobsService.GetPlayerCards());
            _inventory = LoadInventory(blobsService.GetPlayerInventory());
            _combinedRankInfo = LoadCombinedRankInfo(blobsService.GetCombinedRankInfo());
            Load();

            Tournaments = new List<TmpTournament>
            {
                new TmpTournament{ Name = "Mythic Point Challenge 10 Win Decks",
                    Decks =
                    {
                        new TmpDeck { Name = "Sultai Control"},
                        new TmpDeck { Name = "Simic Flash"},
                        new TmpDeck { Name = "Temur Flash"},
                    }
                },
                new TmpTournament{ Name = "Daily Qualifier Week Two @ MagicFest Online",
                    Decks =
                    {
                        new TmpDeck { Name = "Red Deck Wins"},
                        new TmpDeck { Name = "Temur Reclamation"},
                    }
                }
            };
        }

        public ICollection<TmpTournament> Tournaments { get; set; }

        public ICommand ShowPopupCommand { get; set; }

        public ICommand ClosePopupCommand { get; set; }

        public bool DisplayPopup
        {
            get => _displayPopup;
            set => SetProperty(ref _displayPopup, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
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

        private void ValidateDeck(IEnumerable<string> deckList)
        {
            Deck newDeck = ParseDeckList(deckList);
            var missingCards = new List<DeckCard>();
            var validatedDeck = new Deck();
            foreach (DeckCard deckCard in newDeck.MainDeck)
            {
                Card card = _cardsService.Get(deckCard.Set, deckCard.CollectorNumber);

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

            var builder = new StringBuilder();
            builder.AppendLine("Deck:");
            foreach (DeckCard item in result.ValidatedDeck.MainDeck)
            {
                Card card = _cardsService.Get(item.Set, item.CollectorNumber);
                builder.AppendLine($"{item.Count} {card.Title} ({card.Set}) {card.CollectorNumber}");
            }

            builder.AppendLine("");
            builder.AppendLine("Missing:");

            foreach (DeckCard item in result.MissingCards)
            {
                Card card = _cardsService.Get(item.Set, item.CollectorNumber);
                builder.AppendLine($"{item.Count} {card.Title} ({card.Set}) {card.CollectorNumber}");
            }

            builder.AppendLine("");

            builder.AppendLine("Wildcards needed:");
            builder.AppendLine($"Common: {result.Wildcards.Common} ({_inventory.Common})");
            builder.AppendLine($"Uncommon: {result.Wildcards.Uncommon} ({_inventory.Uncommon})");
            builder.AppendLine($"Rare: {result.Wildcards.Rare} ({_inventory.Rare})");
            builder.AppendLine($"MythicRare: {result.Wildcards.MythicRare} ({_inventory.MythicRare})");
            builder.AppendLine("");

            Text = builder.ToString();
        }

        private Dictionary<Card, int> LoadCollection(Blob collectionBlob)
        {
            var collection = new Dictionary<Card, int>();
            foreach (KeyValuePair<string, int> cardInfo in JsonSerializer.Deserialize<Dictionary<string, int>>(collectionBlob.Content))
            {
                Card card = _cardsService.GetById(int.Parse(cardInfo.Key, CultureInfo.InvariantCulture));
                collection.Add(card, cardInfo.Value);
            }

            return collection;
        }

        private Wildcards LoadInventory(Blob inventoryBlob) => JsonSerializer.Deserialize<Wildcards>(inventoryBlob.Content);

        private CombinedRankInfo LoadCombinedRankInfo(Blob inventoryBlob) => JsonSerializer.Deserialize<CombinedRankInfo>(inventoryBlob.Content);

        private void Load()
        {
            Title = $"Rank: {_combinedRankInfo.ConstructedClass} {_combinedRankInfo.ConstructedLevel}";
            string[] deck = new[]
            {
                "2 Chemister's Insight (GRN) 32",
                "1 Cleansing Nova (M19) 9",
                "3 Clifftop Retreat (DAR) 239",
                "3 Crackling Drake (GRN) 163",
                "3 Deafening Clarion (GRN) 165",
                "2 Dive Down (XLN) 53",
                "1 Field of Ruin (XLN) 254",
                "4 Glacial Fortress (XLN) 255",
                "4 Hallowed Fountain (RNA) 251",
                "1 Island (M19) 266",
                "2 Justice Strike (GRN) 182",
                "3 Lava Coil (GRN) 108",
                "1 Mountain (XLN) 275",
                "3 Niv-Mizzet, Parun (GRN) 192",
                "3 Opt (XLN) 65",
                "3 Sacred Foundry (GRN) 254",
                "3 Sarkhan, Fireblood (M19) 154",
                "2 Shock (M19) 156",
                "2 Spell Pierce (XLN) 81",
                "4 Steam Vents (GRN) 257",
                "4 Sulfur Falls (DAR) 247",
                "3 Teferi, Hero of Dominaria (DAR) GR6",
                "3 Treasure Map (XLN) 250",
            };

            ValidateDeck(deck);
        }
    }
}

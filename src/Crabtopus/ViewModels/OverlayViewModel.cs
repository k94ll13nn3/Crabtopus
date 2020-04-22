using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crabtopus.Data;
using Crabtopus.Models;
using Crabtopus.Services;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Crabtopus.ViewModels
{
    internal class OverlayViewModel : ViewModelBase
    {
        private readonly IFetchService _fetchService;
        private readonly Database _database;
        private string _title = "CRABTOPUS";
        private string _popupTitle = "Decks (0)";
        private string _notification = string.Empty;
        private bool _displayPopup;
        private bool _loaded;

        public OverlayViewModel(IFetchService fetchService, Database database)
        {
            _fetchService = fetchService;
            _database = database;

            ShowPopupCommand = new DelegateCommand(() => DisplayPopup = true);
            ClosePopupCommand = new DelegateCommand(() => DisplayPopup = false);
            LoadCommand = new DelegateCommand(async () => await LoadAsync());
            ReloadCommand = new DelegateCommand(async () => await ReloadAsync());
            ExportDeckCommand = new DelegateCommand<Deck>(ExportDeck);
        }

        public ObservableCollection<Tournament> Tournaments { get; } = new ObservableCollection<Tournament>();

        public ICommand ShowPopupCommand { get; set; }

        public ICommand ClosePopupCommand { get; set; }

        public ICommand LoadCommand { get; set; }

        public ICommand ReloadCommand { get; set; }

        public ICommand ExportDeckCommand { get; set; }

        public bool Loaded
        {
            get => _loaded;
            set => SetProperty(ref _loaded, value);
        }

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

        public string PopupTitle
        {
            get => _popupTitle;
            set => SetProperty(ref _popupTitle, value);
        }

        public string Notification
        {
            get => _notification;
            set => SetProperty(ref _notification, value);
        }

        private void ExportDeck(Deck deck)
        {
            string exportedDeck = string.Join(Environment.NewLine, deck.Cards.Where(x => !x.IsSideboard).Select(x => $"{x.Count} {x.Card?.Name}"))
                + Environment.NewLine
                + Environment.NewLine
                + string.Join(Environment.NewLine, deck.Cards.Where(x => x.IsSideboard).Select(x => $"{x.Count} {x.Card?.Name}"));

            Clipboard.SetText(exportedDeck);
            Notification = "Copied!";
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Notification = string.Empty;
            });
        }

        private async Task LoadAsync()
        {
            Loaded = false;
            List<Tournament> tournaments = await _database
                .Tournaments
                .Include(t => t.Decks)
                .ThenInclude(d => d.Cards)
                .ThenInclude(dc => dc.Card)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            Tournaments.Clear();
            foreach (Tournament tournament in tournaments)
            {
                tournament.Decks = tournament.Decks.OrderBy(x => x.Placement).ToList();
                foreach (Deck deck in tournament.Decks)
                {
                    // Sort by sideboard false then true
                    // And by creature, instant/sorcery, other, lands
                    var groupedCards = deck
                        .Cards
                        .Where(c => !c.IsSideboard)
                        .OrderBy(c => c.IsSideboard)
                        .ThenBy(x => x.Card?.TypeList.Min(GetTypePriority))
                        .ThenBy(x => x.Card?.ConvertedManaCost)
                        .ThenBy(x => x.Card?.Name)
                        .GroupBy(x => x.Card?.TypeList.OrderBy(GetTypePriority).First())
                        .ToDictionary(x => $"{x.Key.ToString().Pluralize()} ({x.Sum(c => c.Count)})", x => x.ToList());

                    List<DeckCard> sideboard = deck.Cards.Where(c => c.IsSideboard).ToList();
                    groupedCards[$"Sideboard ({sideboard.Sum(c => c.Count)})"] = sideboard;
                    deck.GroupedCards = groupedCards;
                }

                Tournaments.Add(tournament);
            }

            PopupTitle = $"Decks ({Tournaments.Count})";
            Loaded = true;

            static int GetTypePriority(CardType cardType)
            {
                return cardType switch
                {
                    CardType.Creature => 1,
                    CardType.Instant => 2,
                    CardType.Sorcery => 3,
                    CardType.Artifact => 4,
                    CardType.Enchantment => 5,
                    CardType.Planeswalker => 6,
                    CardType.Land => 7,
                    _ => 100,
                };
            }
        }

        [SuppressMessage("Design", "CA1031", Justification = "Error can come from the base, the website, ... and the app must not crash.")]
        private async Task ReloadAsync()
        {
            try
            {
                Loaded = false;
                ICollection<(int id, string name, int rating, DateTime date)> tournamentInfos = (await _fetchService.GetTournamentsAsync()).ToList();
                foreach ((int id, string name, int rating, DateTime date) in tournamentInfos.OrderByDescending(t => t.date))
                {
                    Tournament? tournament = await _database.Tournaments.FirstOrDefaultAsync(x => x.Id == id);
                    if (tournament is null)
                    {
                        ICollection<Deck> decks = await _fetchService.GetDecksAsync(id);
                        tournament = new Tournament
                        {
                            Id = id,
                            Name = name,
                            Decks = decks,
                            Date = date,
                            Rating = rating
                        };
                        _database.Tournaments.Add(tournament);
                        _database.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                Notification = "Error while fetching decks!";
                await Task.Delay(1000);
                Notification = string.Empty;
            }

            await LoadAsync();
        }
    }
}

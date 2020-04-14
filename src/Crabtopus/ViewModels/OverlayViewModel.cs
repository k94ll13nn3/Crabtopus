using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crabtopus.Data;
using Crabtopus.Models;
using Crabtopus.Services;
using Microsoft.EntityFrameworkCore;

namespace Crabtopus.ViewModels
{
    internal class OverlayViewModel : ViewModelBase
    {
        private readonly IFetchService _fetchService;
        private readonly Database _database;
        private string _title = "CRABTOPUS";
        private string _text = "Decks";
        private string _tooltip = string.Empty;
        private bool _displayPopup;

        public OverlayViewModel(IFetchService fetchService, Database database)
        {
            _fetchService = fetchService;
            _database = database;

            ShowPopupCommand = new DelegateCommand(() => DisplayPopup = true);
            ClosePopupCommand = new DelegateCommand(() => DisplayPopup = false);
            LoadCommand = new DelegateCommand(async () => await LoadAsync());
            ExportDeckCommand = new DelegateCommand<Deck>(ExportDeck);
        }

        public ObservableCollection<Tournament> Tournaments { get; } = new ObservableCollection<Tournament>();

        public ICommand ShowPopupCommand { get; set; }

        public ICommand ClosePopupCommand { get; set; }

        public ICommand LoadCommand { get; set; }

        public ICommand ExportDeckCommand { get; set; }

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

        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

        private void ExportDeck(Deck deck)
        {
            string exportedDeck = string.Join(Environment.NewLine, deck.Cards.Where(x => !x.IsSideboard).Select(x => $"{x.Count} {x.Card?.Name}"))
                + Environment.NewLine
                + Environment.NewLine
                + string.Join(Environment.NewLine, deck.Cards.Where(x => x.IsSideboard).Select(x => $"{x.Count} {x.Card?.Name}"));

            Clipboard.SetText(exportedDeck);
            Tooltip = "Copied!";
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                Tooltip = string.Empty;
            });
        }

        private async Task LoadAsync()
        {
            ICollection<(int id, string name, int rating, DateTime date)> tournamentInfos = (await _fetchService.GetTournamentsAsync()).ToList();
            Text = $"Decks 0/{tournamentInfos.Count}";

            foreach ((int id, string name, int rating, DateTime date) in tournamentInfos)
            {
                Tournament? tournament = await _database
                        .Tournaments
                        .Include(t => t.Decks)
                        .ThenInclude(d => d.Cards)
                        .ThenInclude(dc => dc.Card)
                        .FirstOrDefaultAsync(x => x.Id == id);
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

                tournament.Decks = tournament.Decks.OrderBy(x => x.Placement).ToList();
                foreach (Deck deck in tournament.Decks)
                {
                    // Sort by sideboard false then true
                    // And by creature, instant/sorcery, other, lands
                    deck.Cards = deck
                        .Cards
                        .OrderBy(c => c.IsSideboard)
                        .ThenBy(x => x.Card?.TypeList.Min(x => GetTypePriority(x)))
                        .ThenBy(x => x.Card?.ConvertedManaCost)
                        .ToList();
                }
                Tournaments.Add(tournament);
                Text = $"Decks {Tournaments.Count}/{tournamentInfos.Count}";
            }

            static int GetTypePriority(CardType? cardType)
            {
                return cardType switch
                {
                    CardType.Creature => 0,
                    CardType.Instant => 10,
                    CardType.Sorcery => 10,
                    CardType.Artifact => 20,
                    CardType.Enchantment => 20,
                    CardType.Planeswalker => 20,
                    CardType.Land => 30,
                    CardType.None => 40,
                    _ => 40,
                };
            }
        }
    }
}

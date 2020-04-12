using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _displayPopup;

        public OverlayViewModel()
        {
            Tournaments = new ObservableCollection<Tournament>
            {
                new Tournament
                {
                    Name = "Weekly Championship Day One 8-0 Decks @ MagicFest Online",
                    Rating = 1,
                    Decks = new   List<Deck>
                    {
                        new Deck { Name = "UW Control" },
                    },
                },
                new Tournament
                {
                    Name = "MagicFest Online Weekly Championship",
                    Rating = 4,
                    Decks = new   List<Deck>
                    {
                        new Deck { Name = "Bant Control" },
                        new Deck { Name = "Simic Flash" },
                        new Deck { Name = "Fires of Invention" },
                    },
                },
            };
        }

        public OverlayViewModel(IFetchService fetchService, Database database)
        {
            _fetchService = fetchService;
            _database = database;

            ShowPopupCommand = new DelegateCommand(() => DisplayPopup = true);
            ClosePopupCommand = new DelegateCommand(() => DisplayPopup = false);
            LoadCommand = new DelegateCommand(async () => await LoadAsync());
        }

        public ObservableCollection<Tournament> Tournaments { get; } = new ObservableCollection<Tournament>();

        public ICommand ShowPopupCommand { get; set; }

        public ICommand ClosePopupCommand { get; set; }

        public ICommand LoadCommand { get; set; }

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

        private async Task LoadAsync()
        {
            ICollection<(int id, string name, int rating, DateTime date)>? tournamentInfos = (await _fetchService.GetTournamentsAsync()).ToList();
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

                Tournaments.Add(tournament);
                Text = $"Decks {Tournaments.Count}/{tournamentInfos.Count}";
            }
        }
    }
}

using System;
using System.Collections.Generic;
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
        private string _text = string.Empty;
        private bool _displayPopup;
        private IEnumerable<Tournament> _tournaments = new List<Tournament>();

        public OverlayViewModel(IFetchService fetchService, Database database)
        {
            _fetchService = fetchService;
            _database = database;

            ShowPopupCommand = new DelegateCommand(() => DisplayPopup = true);
            ClosePopupCommand = new DelegateCommand(() => DisplayPopup = false);
            LoadCommand = new DelegateCommand(async () => await LoadAsync());
        }

        public IEnumerable<Tournament> Tournaments
        {
            get => _tournaments;
            set => SetProperty(ref _tournaments, value);
        }

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
            IEnumerable<(int id, string name, int rating, DateTime date)>? tournamentInfos = await _fetchService.GetTournamentsAsync();
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

            }

            Tournaments = await _database.Tournaments.OrderByDescending(x => x.Date).ToListAsync();
        }
    }
}

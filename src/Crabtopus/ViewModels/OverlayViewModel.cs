using System.Collections.Generic;
using System.Windows.Input;
using Crabtopus.Models;

namespace Crabtopus.ViewModels
{
    internal class OverlayViewModel : ViewModelBase
    {
        private string _title = "CRABTOPUS";
        private string _text = string.Empty;
        private bool _displayPopup;

        public OverlayViewModel()
        {
            ShowPopupCommand = new DelegateCommand(() => DisplayPopup = true);
            ClosePopupCommand = new DelegateCommand(() => DisplayPopup = false);

            Tournaments = new List<Tournament>
            {
                new Tournament{ Name = "Mythic Point Challenge 10 Win Decks",
                    Decks =
                    {
                        new Deck { Name = "Sultai Control"},
                        new Deck { Name = "Simic Flash"},
                        new Deck { Name = "Temur Flash"},
                    }
                },
                new Tournament{ Name = "Daily Qualifier Week Two @ MagicFest Online",
                    Decks =
                    {
                        new Deck { Name = "Red Deck Wins"},
                        new Deck { Name = "Temur Reclamation"},
                    }
                }
            };
        }

        public ICollection<Tournament> Tournaments { get; set; }

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
    }
}

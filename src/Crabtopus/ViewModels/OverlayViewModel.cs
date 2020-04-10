using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Crabtopus.Models;
using Crabtopus.Services;

namespace Crabtopus.ViewModels
{
    internal class OverlayViewModel : ViewModelBase
    {
        private string _title = "CRABTOPUS";
        private string _text = string.Empty;
        private bool _displayPopup;
        private IEnumerable<Tournament> _tournaments;

        public OverlayViewModel(IFetchService fetchService)
        {
            ShowPopupCommand = new DelegateCommand(() => DisplayPopup = true);
            ClosePopupCommand = new DelegateCommand(() => DisplayPopup = false);

            Tournaments = new List<Tournament>();
            Task.Run(async () => Tournaments = await fetchService.GetEventsAsync());
        }

        public IEnumerable<Tournament> Tournaments
        {
            get => _tournaments;
            set => SetProperty(ref _tournaments, value);
        }

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

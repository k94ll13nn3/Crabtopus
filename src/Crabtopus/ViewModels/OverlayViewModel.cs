using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crabtopus.ViewModels
{
    internal class OverlayViewModel : ViewModelBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LogReader _logReader;

        public OverlayViewModel(IHttpClientFactory httpClientFactory, LogReader logReader)
        {
            LoadCommand = new DelegateCommand<object>(async _ => await LoadAsync());
            _httpClientFactory = httpClientFactory;
            _logReader = logReader;
        }

        public ICommand LoadCommand { get; set; }

        private async Task LoadAsync()
        {
            var cardManager = new CardManager(_logReader, _httpClientFactory);
            await cardManager.LoadCardsAsync();

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

            var player = new PlayerManager(cardManager, _logReader);
            player.ValidateDeck(deck);
            player.DisplaySeasonStatistics();
        }
    }
}

using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;

namespace Crabtopus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private readonly Overlay _overlay = new Overlay();
        private readonly Overlayer _overlayer;
        private readonly ServiceProvider _serviceProvider;
        private readonly LogReader _logReader;
        private TaskbarIcon? _taskbarIcon;
        private bool _disposedValue;

        public App()
        {
            _overlayer = new Overlayer("firefox", _overlay, OverlayPosition.Top | OverlayPosition.Left);

            _logReader = new LogReader();
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _taskbarIcon?.Dispose();
                    _serviceProvider?.Dispose();
                }

                _disposedValue = true;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Dispose();
            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _taskbarIcon = LoadTaskbarIcon();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (s, e) => Dispatcher.BeginInvoke(new Action(() => _overlayer.Update()));

            timer.Start();

            IHttpClientFactory httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            var cardManager = new CardManager(_logReader, httpClientFactory);
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

        private TaskbarIcon LoadTaskbarIcon()
        {
            return (TaskbarIcon)FindResource("NotifyIcon");
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("mtgarena", c => c.BaseAddress = _logReader.AssetsUri);
        }
    }
}

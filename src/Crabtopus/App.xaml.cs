using System;
using System.Windows;
using System.Windows.Threading;
using Crabtopus.Data;
using Crabtopus.Infrastructure;
using Crabtopus.Models;
using Crabtopus.Services;
using Crabtopus.ViewModels;
using Crabtopus.Views;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Crabtopus
{
    public partial class App : Application, IDisposable
    {
        private IServiceProvider? _serviceProvider;
        private TaskbarIcon? _taskbarIcon;
        private bool _disposedValue;

        public void Dispose()
        {
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

            // Generate configuration.
            IConfigurationRoot configuration = new ConfigurationBuilder().Build();

            // Configure services.
            ConfigureServices(configuration);

            // Initialize database.
            _serviceProvider.GetService<Database>().Database.EnsureCreated();

            // Read cards.
            CardReader cardManager = _serviceProvider.GetService<CardReader>();
            await cardManager.LoadCardsAsync();

            // Initialize tray icon.
            _taskbarIcon = LoadTaskbarIcon();

            // Initialize overlay.
            ConfigureOverlay();
        }

        private void ConfigureOverlay()
        {
            Overlay overlay = _serviceProvider.GetService<Overlay>();
            IOptions<ApplicationSettings> settings = _serviceProvider.GetService<IOptions<ApplicationSettings>>();
            var overlayer = new Overlayer(settings.Value.Process, overlay, OverlayPosition.Top | OverlayPosition.Left);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (s, e) => overlayer.Update();

            timer.Start();
        }

        private void ConfigureServices(IConfigurationRoot configuration)
        {
            // Read MTGA log file.
            var logReader = new LogReader();

            var serviceCollection = new ServiceCollection();

            serviceCollection.Configure<ApplicationSettings>(settings =>
            {
                settings.Process = "firefox";
                settings.SqliteConnectionString = "Data Source=crabtopus.db";
                settings.Endpoint = logReader.Endpoint;
                settings.Version = logReader.Version;
            });

            serviceCollection.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);

            serviceCollection.AddTransient<OverlayViewModel>();
            serviceCollection.AddTransient<Overlay>();
            serviceCollection.AddTransient<ICardsService, CardsService>();
            serviceCollection.AddTransient<IFetchService, FetchService>();

            serviceCollection.AddSingleton<IBlobsService>(logReader);
            serviceCollection.AddSingleton<CardReader>();
            serviceCollection.AddSingleton<Database>();
            serviceCollection.AddSingleton<IConfiguration>(configuration);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private TaskbarIcon LoadTaskbarIcon()
        {
            return (TaskbarIcon)FindResource("NotifyIcon");
        }
    }
}

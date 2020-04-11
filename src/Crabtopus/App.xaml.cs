using System;
using System.IO;
using System.Net.Http;
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

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot? configuration = builder.Build();

            // Read blobs
            var logReader = new LogReader();

            var serviceCollection = new ServiceCollection();
            serviceCollection.Configure<ApplicationSettings>(configuration.GetSection(nameof(ApplicationSettings)));
            serviceCollection.Configure<ApplicationSettings>(settings =>
            {
                settings.Endpoint = logReader.Endpoint;
                settings.Version = logReader.Version;
            });
            serviceCollection.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);
            serviceCollection.AddTransient<OverlayViewModel>();
            serviceCollection.AddTransient<Overlay>();
            serviceCollection.AddSingleton<ICardsService, CardsService>();
            serviceCollection.AddSingleton<IFetchService, FetchService>();
            serviceCollection.AddSingleton<IBlobsService>(logReader);
            serviceCollection.AddSingleton<Database>();
            _serviceProvider = serviceCollection.BuildServiceProvider();

            _serviceProvider.GetService<Database>().Database.EnsureCreated();

            // Read cards
            IOptions<ApplicationSettings> settings = _serviceProvider.GetService<IOptions<ApplicationSettings>>();

            var cardManager = new CardReader(settings, _serviceProvider.GetService<IHttpClientFactory>(), _serviceProvider.GetService<Database>());
            await cardManager.LoadCardsAsync();

            _taskbarIcon = LoadTaskbarIcon();
            Overlay overlay = _serviceProvider.GetService<Overlay>();
            var overlayer = new Overlayer(settings.Value.Process, overlay, OverlayPosition.Top | OverlayPosition.Left);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (s, e) => overlayer.Update();

            timer.Start();
        }

        private TaskbarIcon LoadTaskbarIcon()
        {
            return (TaskbarIcon)FindResource("NotifyIcon");
        }
    }
}

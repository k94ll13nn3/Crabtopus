using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Crabtopus.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crabtopus
{
    public partial class App : Application, IDisposable
    {
        private IServiceProvider? _serviceProvider;
        private TaskbarIcon? _taskbarIcon;
        private IConfiguration? _configuration;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.Configure<ApplicationSettings>(_configuration.GetSection(nameof(ApplicationSettings)));

            // Read blobs
            // Read cards

            var logReader = new LogReader();
            serviceCollection.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);
            serviceCollection.AddTransient<OverlayViewModel>();
            serviceCollection.AddTransient<Overlay>();
            serviceCollection.AddSingleton<LogReader>(); // will be iblobreader
            _serviceProvider = serviceCollection.BuildServiceProvider();

            string? processName = _configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>().Process;

            _taskbarIcon = LoadTaskbarIcon();
            Overlay overlay = _serviceProvider.GetService<Overlay>();
            var overlayer = new Overlayer(processName, overlay, OverlayPosition.Top | OverlayPosition.Left);

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

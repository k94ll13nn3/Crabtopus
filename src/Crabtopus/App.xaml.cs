using System;
using System.Windows;
using System.Windows.Threading;
using Crabtopus.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;

namespace Crabtopus
{
    public partial class App : Application, IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private TaskbarIcon? _taskbarIcon;
        private bool _disposedValue;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _taskbarIcon = LoadTaskbarIcon();
            Overlay overlay = _serviceProvider.GetService<Overlay>();
            var overlayer = new Overlayer("firefox", overlay, OverlayPosition.Top | OverlayPosition.Left);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (s, e) => overlayer.Update();

            timer.Start();
        }

        private TaskbarIcon LoadTaskbarIcon()
        {
            return (TaskbarIcon)FindResource("NotifyIcon");
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var logReader = new LogReader();
            services.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);
            services.AddTransient<OverlayViewModel>();
            services.AddTransient<Overlay>();
            services.AddSingleton<LogReader>();
        }
    }
}

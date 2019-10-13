using System;
using System.Threading.Tasks;
using Crabtopus.App.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Crabtopus.App
{
    public class Startup
    {
        public Startup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json");

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == Environments.Development)
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public static async Task RunAsync()
        {
            var startup = new Startup();
            await startup.RunInternalAsync();
        }

        private async Task RunInternalAsync()
        {
            var services = new ServiceCollection();
            var discordSocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            };

            var commandServiceConfig = new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
            };

            services.AddTransient<FetchService>();
            services.AddSingleton(_ => new DiscordSocketClient(discordSocketConfig));
            services.AddSingleton(_ => new CommandService(commandServiceConfig));
            services.AddSingleton<CommandHandler>();
            services.AddSingleton<StartupService>();
            services.AddSingleton<LoggingService>();
            services.AddSingleton<CardsService>();
            services.AddSingleton(Configuration);

            services.AddHttpClient("mtgarena", c => c.BaseAddress = new Uri(Configuration["AssetsUri"]));

            ServiceProvider provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandler>();

            await provider.GetRequiredService<StartupService>().StartAsync();
            await Task.Delay(-1);
        }
    }
}

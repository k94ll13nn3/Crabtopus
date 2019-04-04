using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Crabtopus.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Crabtopus
{
    public static class Program
    {
        public static async Task Main()
        {
            var logReader = new LogReader();

            var services = new ServiceCollection();
            services.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IHttpClientFactory httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            var cardManager = new CardManager(logReader, httpClientFactory);
            await cardManager.LoadCardsAsync();

            var player = new PlayerManager(cardManager, logReader);
            ValidationResult validationResult = player.ValidateDeck(File.ReadAllLines("deck.txt"));
        }
    }
}

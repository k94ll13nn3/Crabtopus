using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Crabtopus
{
    public static class Program
    {
        public static async Task Main()
        {
            var logReader = new LogReader();
            logReader.ReadLog();

            var services = new ServiceCollection();
            services.AddHttpClient("mtgarena", c => c.BaseAddress = logReader.AssetsUri);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IHttpClientFactory httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            var cardManager = new CardManager(logReader.Version, logReader.Endpoint, httpClientFactory);
            await cardManager.InitializeCardsAsync();

            var player = new PlayerManager(cardManager);
            player.LoadCollection(logReader.Blobs.First(x => x.Method == "GetPlayerCardsV3"));
            player.LoadInventory(logReader.Blobs.First(x => x.Method == "GetPlayerInventory"));
            player.ValidateDeck(File.ReadAllLines("deck.txt"));
        }
    }
}

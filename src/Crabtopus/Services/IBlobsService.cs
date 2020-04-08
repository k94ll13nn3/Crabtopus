using Crabtopus.Models;

namespace Crabtopus.Services
{
    internal interface IBlobsService
    {
        Blob GetPlayerCards();

        Blob GetPlayerInventory();

        Blob GetCombinedRankInfo();
    }
}

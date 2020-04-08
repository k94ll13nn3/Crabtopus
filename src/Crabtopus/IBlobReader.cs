using Crabtopus.Models;

namespace Crabtopus
{
    internal interface IBlobReader
    {
        Blob GetPlayerCards();

        Blob GetPlayerInventory();

        Blob GetCombinedRankInfo();
    }
}

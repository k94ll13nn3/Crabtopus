using Crabtopus.Models;

namespace Crabtopus.Services
{
    internal interface ICardsService
    {
        Card GetById(string id);

        Card Get(string setCode, string collectorNumber);
    }
}

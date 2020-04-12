using Crabtopus.Models;

namespace Crabtopus.Services
{
    internal interface ICardsService
    {
        Card GetById(int id);

        Card Get(string setCode, string collectorNumber);
    }
}

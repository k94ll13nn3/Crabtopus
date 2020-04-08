using Crabtopus.Models;

namespace Crabtopus
{
    internal interface ICardRepository
    {
        Card GetById(string id);

        Card Get(string setCode, string collectorNumber);
    }
}

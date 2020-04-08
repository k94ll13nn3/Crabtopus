using Crabtopus.Data;
using Crabtopus.Models;

namespace Crabtopus.Services
{
    internal class CardsService : ICardsService
    {
        private readonly Database _database;

        public CardsService(Database database)
        {
            _database = database;
        }

        public Card Get(string setCode, string collectorNumber)
        {
            return _database.Set<Card>().FindOne(x => x.Set == setCode && x.CollectorNumber == collectorNumber);
        }

        public Card GetById(int id)
        {
            return _database.Set<Card>().FindById(id);
        }
    }
}

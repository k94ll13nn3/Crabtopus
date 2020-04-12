using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crabtopus.Models;

namespace Crabtopus.Services
{
    internal interface IFetchService
    {
        Task<IEnumerable<(int id, string name, int rating, DateTime date)>> GetTournamentsAsync();

        Task<ICollection<Deck>> GetDecksAsync(int eventId);
    }
}

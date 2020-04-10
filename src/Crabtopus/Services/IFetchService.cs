using System.Collections.Generic;
using System.Threading.Tasks;
using Crabtopus.Models;

namespace Crabtopus.Services
{
    internal interface IFetchService
    {
        Task<IEnumerable<Tournament>> GetEventsAsync();
    }
}

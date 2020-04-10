using System.Collections.Generic;

namespace Crabtopus.Models
{
    public class Tournament
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<Deck> Decks { get; } = new List<Deck>();
    }
}

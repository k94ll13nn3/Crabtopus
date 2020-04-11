using System;
using System.Collections.Generic;

namespace Crabtopus.Models
{
    internal class Tournament
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public int Rating { get; set; }

        public ICollection<Deck> Decks { get; set; } = new List<Deck>();
    }
}

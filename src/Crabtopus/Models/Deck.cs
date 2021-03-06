﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crabtopus.Models
{
    internal class Deck
    {
        public int Id { get; set; }

        public Tournament? Tournament { get; set; }

        public int TournamentId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public string Placement { get; set; } = string.Empty;

        public ICollection<DeckCard> Cards { get; set; } = new List<DeckCard>();

        [NotMapped]
        public IDictionary<string, List<DeckCard>> GroupedCards { get; set; } = new Dictionary<string, List<DeckCard>>();
    }
}

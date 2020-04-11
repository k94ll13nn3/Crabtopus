using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Crabtopus.Models
{
    internal class Deck
    {
        public int Id { get; set; }

        public Tournament Tournament { get; set; }

        public int TournamentId { get; set; }

        public string Name { get; set; }

        public string User { get; set; }

        public string Placement { get; set; }

        public ICollection<DeckCard> Cards { get; set; }

        [NotMapped]
        public string Tooltip =>
            string.Join(Environment.NewLine, Cards.Where(x => !x.IsSideboard).Select(x => $"{x.Count} {x.Card.Title}"))
            + Environment.NewLine
            + Environment.NewLine
            + string.Join(Environment.NewLine, Cards.Where(x => x.IsSideboard).Select(x => $"{x.Count} {x.Card.Title}"));
    }
}

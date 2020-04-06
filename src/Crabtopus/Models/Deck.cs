using System.Collections.Generic;

namespace Crabtopus.Models
{
    internal class Deck
    {
        public ICollection<DeckCard> MainDeck { get; set; } = new List<DeckCard>();

        public ICollection<DeckCard> Sideboard { get; set; } = new List<DeckCard>();
    }
}

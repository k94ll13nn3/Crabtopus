using System.Collections.Generic;

namespace Crabtopus.Model
{
    public class Deck
    {
        public ICollection<DeckCard> MainDeck { get; set; }

        public ICollection<DeckCard> Sideboard { get; set; }
    }
}

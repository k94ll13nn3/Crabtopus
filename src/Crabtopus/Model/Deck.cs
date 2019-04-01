using System.Collections.Generic;

namespace Crabtopus.Model
{
    public class Deck
    {
        public Dictionary<Card, int> MainDeck { get; set; }

        public Dictionary<Card, int> Sideboard { get; set; }
    }
}

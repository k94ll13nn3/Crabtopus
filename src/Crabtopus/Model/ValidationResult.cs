using System.Collections.Generic;

namespace Crabtopus.Model
{
    public class ValidationResult
    {
        public Deck ValidatedDeck { get; set; }

        public ICollection<DeckCard> MissingCards { get; set; }

        public Wildcards Wildcards { get; set; }
    }
}

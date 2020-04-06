using System.Collections.Generic;

namespace Crabtopus.Model
{
    internal class ValidationResult
    {
        public ValidationResult(Deck validatedDeck, ICollection<DeckCard> missingCards, Wildcards wildcards)
        {
            ValidatedDeck = validatedDeck;
            MissingCards = missingCards;
            Wildcards = wildcards;
        }

        public Deck ValidatedDeck { get; }

        public ICollection<DeckCard> MissingCards { get; }

        public Wildcards Wildcards { get; }
    }
}

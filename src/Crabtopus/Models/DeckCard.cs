namespace Crabtopus.Models
{
    internal class DeckCard
    {
        public int DeckId { get; set; }

        public Deck? Deck { get; set; }

        public int Count { get; set; }

        public int CardId { get; set; }

        public Card? Card { get; set; }

        public bool IsSideboard { get; set; }
    }
}

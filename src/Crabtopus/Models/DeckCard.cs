namespace Crabtopus.Models
{
    internal class DeckCard
    {
        public string CollectorNumber { get; set; } = string.Empty;

        public string Set { get; set; } = string.Empty;

        public Rarity Rarity { get; set; }

        public int Count { get; set; }
    }
}

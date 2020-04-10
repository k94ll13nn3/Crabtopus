using Crabtopus.Data;

namespace Crabtopus.Models
{
    internal class Card : IEntity
    {
        public int Id { get; set; }

        public string CollectorNumber { get; set; } = string.Empty;

        public string Set { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public Rarity Rarity { get; set; }
    }
}

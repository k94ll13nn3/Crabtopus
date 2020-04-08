using Crabtopus.Data;

namespace Crabtopus.Models
{
    internal class GameInfo : IEntity
    {
        public int Id { get; set; }

        public string Version { get; set; } = string.Empty;

        public string CardsHash { get; set; } = string.Empty;

        public string LocalizationHash { get; set; } = string.Empty;
    }
}

using System.Text.Json.Serialization;

namespace Crabtopus.Models.Json
{
    internal class CardInfo
    {
        [JsonPropertyName("grpid")]
        public int Id { get; set; }

        [JsonPropertyName("titleId")]
        public long TitleId { get; set; }

        [JsonPropertyName("CollectorNumber")]
        public string CollectorNumber { get; set; } = string.Empty;

        [JsonPropertyName("set")]
        public string Set { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("rarity")]
        public int RarityValue { get; set; }
    }
}

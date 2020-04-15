using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Crabtopus.Models.Json
{
    internal class CardInfo
    {
        [JsonPropertyName("grpid")]
        public int Id { get; set; }

        [JsonPropertyName("titleId")]
        public long TitleId { get; set; }

        [JsonPropertyName("isToken")]
        public bool IsToken { get; set; }

        [JsonPropertyName("CollectorNumber")]
        public string CollectorNumber { get; set; } = string.Empty;

        [JsonPropertyName("set")]
        public string Set { get; set; } = string.Empty;

        [JsonIgnore]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("rarity")]
        public int RarityValue { get; set; }

        [JsonPropertyName("colors")]
        public List<int> Colors { get; set; } = new List<int>();

        [JsonPropertyName("castingcost")]
        public string Cost { get; set; } = string.Empty;

        [JsonPropertyName("cmc")]
        public int ConvertedManaCost { get; set; }

        [JsonPropertyName("types")]
        public List<int> Types { get; set; } = new List<int>();

        [JsonPropertyName("isCollectible")]
        public bool IsCollectible { get; set; }

        [JsonPropertyName("linkedFaceType")]
        public int LinkedFaceType { get; set; }

        [JsonPropertyName("linkedFaces")]
        public List<int> LinkedFaces { get; set; } = new List<int>();
    }
}

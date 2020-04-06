using System.Text.Json.Serialization;

namespace Crabtopus.Model
{
    internal class CombinedRankInfo
    {
        [JsonPropertyName("constructedClass")]
        public string ConstructedClass { get; set; } = string.Empty;

        [JsonPropertyName("constructedLevel")]
        public long ConstructedLevel { get; set; }

        [JsonPropertyName("constructedStep")]
        public long ConstructedStep { get; set; }

        [JsonPropertyName("constructedMatchesWon")]
        public long ConstructedMatchesWon { get; set; }

        [JsonPropertyName("constructedMatchesLost")]
        public long ConstructedMatchesLost { get; set; }

        [JsonPropertyName("constructedMatchesDrawn")]
        public long ConstructedMatchesDrawn { get; set; }

        [JsonIgnore]
        public long ConstructedMatchesTotal => ConstructedMatchesWon + ConstructedMatchesLost + ConstructedMatchesDrawn;
    }
}

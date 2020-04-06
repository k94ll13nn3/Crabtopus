using Newtonsoft.Json;

namespace Crabtopus.Model
{
    internal class CombinedRankInfo
    {
        public string ConstructedClass { get; set; } = string.Empty;

        public long ConstructedLevel { get; set; }

        public long ConstructedStep { get; set; }

        public long ConstructedMatchesWon { get; set; }

        public long ConstructedMatchesLost { get; set; }

        public long ConstructedMatchesDrawn { get; set; }

        [JsonIgnore]
        public long ConstructedMatchesTotal => ConstructedMatchesWon + ConstructedMatchesLost + ConstructedMatchesDrawn;
    }
}

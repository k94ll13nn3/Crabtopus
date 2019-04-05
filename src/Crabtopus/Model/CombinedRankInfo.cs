using Newtonsoft.Json;

namespace Crabtopus.Model
{
    public class CombinedRankInfo
    {
        public string ConstructedClass { get; set; }

        public long ConstructedLevel { get; set; }

        public long ConstructedStep { get; set; }

        public long ConstructedMatchesWon { get; set; }

        public long ConstructedMatchesLost { get; set; }

        public long ConstructedMatchesDrawn { get; set; }

        [JsonIgnore]
        public long ConstructedMatchesTotal => ConstructedMatchesWon + ConstructedMatchesLost + ConstructedMatchesDrawn;
    }
}

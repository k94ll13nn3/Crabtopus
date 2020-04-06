using Newtonsoft.Json;

namespace Crabtopus.Model
{
    internal class Wildcards
    {
        [JsonProperty("wcCommon")]
        public long Common { get; set; }

        [JsonProperty("wcUncommon")]
        public long Uncommon { get; set; }

        [JsonProperty("wcRare")]
        public long Rare { get; set; }

        [JsonProperty("wcMythic")]
        public long MythicRare { get; set; }
    }
}

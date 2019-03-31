using Newtonsoft.Json;

namespace Crabtopus
{
    public class Inventory
    {
        [JsonProperty("wcCommon")]
        public long CommonWildcards { get; set; }

        [JsonProperty("wcUncommon")]
        public long UncommonWildcards { get; set; }

        [JsonProperty("wcRare")]
        public long RareWildcards { get; set; }

        [JsonProperty("wcMythic")]
        public long MythicWildcards { get; set; }
    }
}


using System.Text.Json.Serialization;

namespace Crabtopus.Models
{
    internal class Wildcards
    {
        [JsonPropertyName("wcCommon")]
        public long Common { get; set; }

        [JsonPropertyName("wcUncommon")]
        public long Uncommon { get; set; }

        [JsonPropertyName("wcRare")]
        public long Rare { get; set; }

        [JsonPropertyName("wcMythic")]
        public long MythicRare { get; set; }
    }
}

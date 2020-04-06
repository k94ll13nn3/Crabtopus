using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Crabtopus.Models
{
    internal class Localization
    {
        [JsonPropertyName("isoCode")]
        public string IsoCode { get; set; } = string.Empty;

        [JsonPropertyName("keys")]
        public List<Key> Keys { get; set; } = new List<Key>();
    }
}

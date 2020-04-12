using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Crabtopus.Models.Json
{
    internal class Manifest
    {
        [JsonPropertyName("Assets")]
        public List<Asset> Assets { get; set; } = new List<Asset>();
    }
}

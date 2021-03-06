﻿using System.Text.Json.Serialization;

namespace Crabtopus.Models.Json
{
    internal class Asset
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Hash")]
        public string Hash { get; set; } = string.Empty;
    }
}

﻿using System.Text.Json.Serialization;

namespace Crabtopus.Models.Json
{
    internal class Key
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}

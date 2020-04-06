using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crabtopus.Model
{
    internal class Card : IEquatable<Card>
    {
        [JsonProperty("grpid")]
        public long Id { get; set; }

        public long TitleId { get; set; }

        public string CollectorNumber { get; set; } = string.Empty;

        public string Set { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        [JsonProperty("rarity")]
        public int RarityValue { get; set; }

        [JsonIgnore]
        public Rarity Rarity
        {
            get
            {
                return RarityValue switch
                {
                    2 => Rarity.Common,
                    3 => Rarity.Uncommon,
                    4 => Rarity.Rare,
                    5 => Rarity.MythicRare,
                    _ => Rarity.BasicLand,
                };
            }
        }

        public static bool operator ==(Card? left, Card? right) => EqualityComparer<Card>.Default.Equals(left, right);

        public static bool operator !=(Card? left, Card? right) => !(left == right);

        public override bool Equals(object? obj) => Equals(obj as Card);

        public bool Equals(Card? other) => other != null && Id == other.Id && TitleId == other.TitleId && CollectorNumber == other.CollectorNumber && Set == other.Set && Title == other.Title && RarityValue == other.RarityValue && Rarity == other.Rarity;

        public override int GetHashCode() => HashCode.Combine(Id, TitleId, CollectorNumber, Set, Title, RarityValue, Rarity);
    }
}

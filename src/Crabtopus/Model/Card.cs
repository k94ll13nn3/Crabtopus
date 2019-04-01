using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crabtopus.Model
{
    public class Card : IEquatable<Card>
    {
        [JsonProperty("grpid")]
        public long Id { get; set; }

        public long TitleId { get; set; }

        public string CollectorNumber { get; set; }

        public string Set { get; set; }

        public string Title { get; set; }

        [JsonProperty("rarity")]
        public int RarityValue { get; set; }

        [JsonIgnore]
        public Rarity Rarity
        {
            get
            {
                switch (RarityValue)
                {
                    case 2:
                        return Rarity.Common;

                    case 3:
                        return Rarity.Uncommon;

                    case 4:
                        return Rarity.Rare;

                    case 5:
                        return Rarity.MythicRare;
                }

                return Rarity.BasicLand;
            }
        }

        public override bool Equals(object obj) => Equals(obj as Card);
        public bool Equals(Card other) => other != null && Id == other.Id && TitleId == other.TitleId && CollectorNumber == other.CollectorNumber && Set == other.Set && Title == other.Title && RarityValue == other.RarityValue && Rarity == other.Rarity;
        public override int GetHashCode() => HashCode.Combine(Id, TitleId, CollectorNumber, Set, Title, RarityValue, Rarity);

        public static bool operator ==(Card left, Card right) => EqualityComparer<Card>.Default.Equals(left, right);
        public static bool operator !=(Card left, Card right) => !(left == right);
    }
}

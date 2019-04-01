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

        public int Rarity { get; set; }

        public static bool operator ==(Card left, Card right) => EqualityComparer<Card>.Default.Equals(left, right);

        public static bool operator !=(Card left, Card right) => !(left == right);

        public override bool Equals(object obj) => Equals(obj as Card);

        public bool Equals(Card other) => other != null && CollectorNumber == other.CollectorNumber && Set == other.Set;

        public override int GetHashCode() => HashCode.Combine(CollectorNumber, Set);
    }
}

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crabtopus.App.Model
{
    public class CardData
    {
        [JsonProperty("grpid")]
        public long Id { get; set; }

        public long TitleId { get; set; }

        public string CollectorNumber { get; set; }

        public string Set { get; set; }

        public string Title { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "Needed for deserialization.")]
        public List<CardColor> Colors { get; set; }

        public Rarity Rarity { get; set; }
    }
}

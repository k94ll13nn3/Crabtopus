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
    }
}

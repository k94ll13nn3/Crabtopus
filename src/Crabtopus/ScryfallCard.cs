using Newtonsoft.Json;

namespace Crabtopus
{
    public class ScryfallCard
    {
        [JsonProperty("arena_id")]
        public long ArenaId { get; set; }

        public string Name { get; set; }

        public string Set { get; set; }

        [JsonProperty("collector_number")]
        public string CollectorNumber { get; set; }
    }
}

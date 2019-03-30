using Newtonsoft.Json;

namespace Crabtopus
{
    public class Card
    {
        [JsonProperty("Grpid")]

        public long Id { get; set; }

        public long TitleId { get; set; }

        public string CollectorNumber { get; set; }

        public string Set { get; set; }

        public string Title { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace Crabtopus.Models.Json
{
    internal class DataResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("payload")]
        public object? Payload { get; set; }
    }
}

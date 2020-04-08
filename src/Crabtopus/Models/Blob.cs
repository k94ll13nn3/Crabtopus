using System.Diagnostics;
using System.Text.Json;

namespace Crabtopus.Models
{
    [DebuggerDisplay("{Name}")]
    internal class Blob
    {
        public Blob(string type, string method, string data)
        {
            Type = type;
            Method = method;
            DataResponse response = JsonSerializer.Deserialize<DataResponse>(data);
            Id = response.Id;
            Content = response.Payload?.ToString() ?? string.Empty;
            IsArray = Content.Length > 0 && Content[0] == '[';
        }

        public string Type { get; }

        public string Method { get; }

        public int Id { get; }

        public string Name => $"{Type}.{Method}";

        public string FullName => $"{Type}.{Method}({Id})";

        public bool IsArray { get; }

        public string Content { get; }
    }
}

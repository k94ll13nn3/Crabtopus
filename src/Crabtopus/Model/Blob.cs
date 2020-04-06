using System.Diagnostics;
using System.Text.Json;

namespace Crabtopus.Model
{
    [DebuggerDisplay("{Name}")]
    internal class Blob
    {
        public Blob(string data)
        {
            string[] splittedData = data.Split(new[] { '.', ' ' }, 3);
            Type = splittedData[0];
            Method = splittedData[1];
            DataResponse response = JsonSerializer.Deserialize<DataResponse>(splittedData[2]);
            Id = response.Id;
            Content = response.Payload?.ToString() ?? string.Empty;
            IsArray = Content[0] == '[';
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

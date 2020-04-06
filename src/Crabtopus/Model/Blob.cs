using System.Diagnostics;
using Newtonsoft.Json;

namespace Crabtopus.Model
{
    [DebuggerDisplay("{Name}")]
    public class Blob
    {
        public Blob(string data)
        {
            string[] splittedData = data.Split(new[] { '.', ' ' }, 3);
            Type = splittedData[0];
            Method = splittedData[1];
            DataResponse response = JsonConvert.DeserializeObject<DataResponse>(splittedData[2]);
            Id = response.Id;
            Content = response.Payload.ToString();
            IsArray = Content[0] == '[';
        }

        public string Type { get; set; }

        public string Method { get; set; }

        public int Id { get; set; }

        public string Name => $"{Type}.{Method}";

        public string FullName => $"{Type}.{Method}({Id})";

        public bool IsArray { get; set; }

        public string Content { get; set; }
    }
}

namespace Crabtopus.Models
{
    internal class Blob
    {
        public Blob(string type, string method, int id, string content)
        {
            Type = type;
            Method = method;
            Id = id;
            Content = content;
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

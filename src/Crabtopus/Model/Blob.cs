namespace Crabtopus.Model
{
    public class Blob
    {
        public Blob(string data, bool isArray, string content)
        {
            string[] splittedData = data.Split('.', '(', ')');
            Type = splittedData[0];
            Method = splittedData[1];
            Id = int.Parse(splittedData[2]);
            IsArray = isArray;
            Content = content;
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

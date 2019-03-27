namespace Crabtopus
{
    internal class Blob
    {
        public Blob(string name, bool isArray)
        {
            Name = name;
            IsArray = isArray;
        }

        public string Name { get; set; }

        public bool IsArray { get; set; }
    }
}

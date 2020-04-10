namespace Crabtopus.Models
{
    internal class DeckCard
    {
        public DeckCard(int count, string name)
        {
            Count = count;
            Name = name;
        }

        public int Count { get; }

        public string Name { get; }
    }
}

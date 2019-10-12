namespace Crabtopus.App.Model
{
    public class Card
    {
        public Card(int count, string name)
        {
            Count = count;
            Name = name;
        }

        public int Count { get; }

        public string Name { get; }
    }
}

namespace Crabtopus.Core.Model
{
    public class EventDeck
    {
        public EventDeck(int id, string name, string user, string placement)
        {
            Id = id;
            Name = name;
            User = user;
            Placement = placement;
        }

        public int Id { get; }

        public string Name { get; }

        public string User { get; }

        public string Placement { get; }
    }
}

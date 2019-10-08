namespace Crabtopus
{
    internal class Event
    {
        public Event(int id, string name, string date, int rating)
        {
            Id = id;
            Name = name;
            Date = date;
            Rating = rating;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Date { get; set; }

        public int Rating { get; set; }
    }
}

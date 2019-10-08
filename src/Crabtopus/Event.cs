using System;

namespace Crabtopus
{
    internal class Event
    {
        public Event(int id, string name, DateTime date, int rating)
        {
            Id = id;
            Name = name;
            Date = date;
            Rating = rating;
        }

        public int Id { get; }

        public string Name { get; }

        public DateTime Date { get; }

        public int Rating { get; }
    }
}

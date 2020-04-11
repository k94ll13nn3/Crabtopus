using System;
using System.Collections.Generic;
using Crabtopus.Data;

namespace Crabtopus.Models
{
    internal class Tournament : IEntity
    {
        public Tournament(int id, string name, DateTime date, int rating, IEnumerable<Deck> decks)
        {
            Id = id;
            Name = name;
            Date = date;
            Rating = rating;
            Decks = decks;
        }

        public int Id { get; }

        public string Name { get; }

        public DateTime Date { get; }

        public int Rating { get; }

        public IEnumerable<Deck> Decks { get; }
    }
}

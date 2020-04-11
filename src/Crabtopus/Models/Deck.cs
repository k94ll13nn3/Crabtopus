using System;
using System.Collections.Generic;
using System.Linq;
using Crabtopus.Data;
using LiteDB;

namespace Crabtopus.Models
{
    internal class Deck : IEntity
    {
        public Deck(int id, string name, string user, string placement, IEnumerable<DeckCard> maindeck, IEnumerable<DeckCard> sideboard)
        {
            Id = id;
            Name = name;
            User = user;
            Placement = placement;
            Maindeck = maindeck;
            Sideboard = sideboard;
        }

        public int Id { get; }

        public string Name { get; }

        public string User { get; }

        public string Placement { get; }

        [BsonIgnore]
        public string Tooltip =>
            string.Join(Environment.NewLine, Maindeck.Select(x => $"{x.Count} {x.Name}"))
            + Environment.NewLine
            + Environment.NewLine
            + string.Join(Environment.NewLine, Sideboard.Select(x => $"{x.Count} {x.Name}"));

        public IEnumerable<DeckCard> Maindeck { get; }

        public IEnumerable<DeckCard> Sideboard { get; }
    }
}

using System.Collections.Generic;

namespace Crabtopus.App.Model
{
    public class Deck
    {
        public Deck(string name, IEnumerable<Card> maindeck, IEnumerable<Card> sideboard)
        {
            Name = name;
            Maindeck = maindeck;
            Sideboard = sideboard;
        }

        public string Name { get; }

        public IEnumerable<Card> Maindeck { get; }

        public IEnumerable<Card> Sideboard { get; }
    }
}
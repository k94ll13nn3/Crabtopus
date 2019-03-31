using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Crabtopus
{
    public class Deck
    {
        private static readonly Regex _cardLineRegex = new Regex(@"(\d{1,2}) (.*?) \((\w+)\) (\w+)", RegexOptions.Compiled);

        public Dictionary<Card, int> MainDeck { get; set; }

        public List<Card> Sideboard { get; set; }

        public static Deck ParseDeckList(IEnumerable<string> deckList)
        {
            var deck = new Deck
            {
                MainDeck = new Dictionary<Card, int>()
            };

            foreach (string line in deckList)
            {
                Match match = _cardLineRegex.Match(line);
                if (match.Success)
                {
                    var card = new Card { Title = match.Groups[2].Value, Set = match.Groups[3].Value, CollectorNumber = match.Groups[4].Value };
                    if (deck.MainDeck.ContainsKey(card))
                    {
                        deck.MainDeck[card] += int.Parse(match.Groups[1].Value);
                    }
                    else
                    {
                        deck.MainDeck[card] = int.Parse(match.Groups[1].Value);
                    }
                }
            }

            return deck;
        }
    }
}

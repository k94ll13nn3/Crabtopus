using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Crabtopus.Models
{
    internal class Card
    {
        private string _colors = string.Empty;

        private string _types = string.Empty;

        public int Id { get; set; }

        public string CollectorNumber { get; set; } = string.Empty;

        public string Set { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public Rarity Rarity { get; set; }

        public string Colors
        {
            get { return _colors; }
            set
            {
                _colors = value;
                if (!string.IsNullOrWhiteSpace(_colors))
                {
                    ColorList = _colors.Split(';').Select(Enum.Parse<CardColor>).ToList();
                }
            }
        }

        public string Types
        {
            get { return _types; }
            set
            {
                _types = value;
                if (!string.IsNullOrWhiteSpace(_types))
                {
                    TypeList = _types.Split(';').Select(Enum.Parse<CardType>).ToList();
                }
            }
        }

        public string Cost { get; set; } = string.Empty;

        public int ConvertedManaCost { get; set; }

        [NotMapped]
        public IEnumerable<CardColor> ColorList { get; private set; } = Enumerable.Empty<CardColor>();

        [NotMapped]
        public IEnumerable<CardType> TypeList { get; private set; } = Enumerable.Empty<CardType>();
    }
}

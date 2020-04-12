using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Crabtopus.Models
{
    internal class Card
    {
        public int Id { get; set; }

        public string CollectorNumber { get; set; } = string.Empty;

        public string Set { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public Rarity Rarity { get; set; }

        public string Colors { get; set; } = string.Empty;

        public string Cost { get; set; } = string.Empty;

        public int ConvertedManaCost { get; set; }

        [NotMapped]
        public IEnumerable<Color> CColors => Colors.Split(';').Select(Enum.Parse<Color>);
    }
}

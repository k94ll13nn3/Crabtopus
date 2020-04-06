using System.Collections.Generic;

namespace Crabtopus.Model
{
    internal class Localization
    {
        public string IsoCode { get; set; } = string.Empty;

        public List<Key> Keys { get; set; } = new List<Key>();
    }
}

using System.Collections.Generic;

namespace Crabtopus.App.Model
{
    public class Localization
    {
        public string Langkey { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "Needed for deserialization.")]
        public List<Key> Keys { get; set; }
    }
}

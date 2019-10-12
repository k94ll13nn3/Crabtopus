using System.Collections.Generic;

namespace Crabtopus.App.Model
{
    public class Manifest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "Needed for deserialization.")]
        public List<Asset> Assets { get; set; }
    }
}

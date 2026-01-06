using System.Collections.Generic;

namespace WeLauncher.Models
{
    public class Manifest
    {
        public int SchemaVersion { get; set; }
        public List<AppDescriptor> Apps { get; set; } = new();
    }
}


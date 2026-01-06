using System.Collections.Generic;

namespace WeLauncher.Models
{
    public class AppDescriptor
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public bool Visible { get; set; }
        public string DownloadUrl { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string WrapperRelativePath { get; set; } = "";
        public List<string> LaunchArgs { get; set; } = new();
        public Dictionary<string, string> Env { get; set; } = new();
    }
}


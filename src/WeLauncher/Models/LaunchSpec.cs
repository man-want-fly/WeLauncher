using System.Collections.Generic;

namespace WeLauncher.Models
{
    public class LaunchSpec
    {
        public string AppId { get; set; } = "";
        public string AppDir { get; set; } = "";
        public List<string> Args { get; set; } = new();
        public Dictionary<string, string> Env { get; set; } = new();
    }
}


using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WeLauncher.Models;

namespace WeLauncher.Services
{
    public class LaunchService
    {
        public string CreateLaunchToken(string baseDir, string appId)
        {
            var launchDir = Path.Combine(baseDir, "launch");
            Directory.CreateDirectory(launchDir);
            var guid = System.Guid.NewGuid().ToString("N");
            var tokenFile = Path.Combine(launchDir, $"{appId}-{guid}.ok");
            File.WriteAllText(tokenFile, System.DateTime.UtcNow.ToString("O"));
            return guid;
        }

        public void WriteLaunchSpec(string baseDir, LaunchSpec spec, string guid)
        {
            var launchDir = Path.Combine(baseDir, "launch");
            Directory.CreateDirectory(launchDir);
            var specFile = Path.Combine(launchDir, $"{spec.AppId}-{guid}.json");
            var json = JsonSerializer.Serialize(spec);
            File.WriteAllText(specFile, json);
        }

        public Process StartWrapper(string wrapperExe, string appId, string guid, string appDir)
        {
            var psi = new ProcessStartInfo(wrapperExe);
            psi.ArgumentList.Add($"--appId={appId}");
            psi.ArgumentList.Add($"--token={guid}");
            psi.ArgumentList.Add($"--appDir={appDir}");
            psi.UseShellExecute = false;
            return Process.Start(psi);
        }
    }
}

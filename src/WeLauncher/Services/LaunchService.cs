using System.Diagnostics;
using System.IO;

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


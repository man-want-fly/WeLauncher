using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WeLauncher.Models;

namespace WeLauncher.Services
{
    public class LaunchService
    {
        public Process? StartWrapper(string wrapperExe, string appDir)
        {
            var psi = new ProcessStartInfo(wrapperExe);
            // Pass appDir so the wrapper knows where the app root is
            psi.ArgumentList.Add($"--appDir={appDir}");
            psi.UseShellExecute = false;
            return Process.Start(psi);
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;

namespace MinimalWrapper
{
    class Program
    {
        static int Main(string[] args)
        {
            string appId = "";
            string token = "";
            string appDir = "";
            foreach (var a in args)
            {
                if (a.StartsWith("--appId=")) appId = a.Substring(8);
                else if (a.StartsWith("--token=")) token = a.Substring(8);
                else if (a.StartsWith("--appDir=")) appDir = a.Substring(9);
            }
            if (!IsShellRunning("WeLauncher")) return 1;
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WeLauncher");
            if (!VerifyToken(baseDir, appId, token, TimeSpan.FromMinutes(2))) return 2;
            var payloadExe = Path.Combine(appDir, "app", "App.exe");
            if (!File.Exists(payloadExe)) return 3;
            var p = StartPayload(payloadExe);
            p?.WaitForExit();
            return 0;
        }

        static bool IsShellRunning(string name)
        {
            foreach (var p in Process.GetProcessesByName(name))
            {
                if (!p.HasExited) return true;
            }
            return false;
        }

        static bool VerifyToken(string baseDir, string appId, string guid, TimeSpan ttl)
        {
            var tokenFile = Path.Combine(baseDir, "launch", $"{appId}-{guid}.ok");
            if (!File.Exists(tokenFile)) return false;
            var ts = DateTime.Parse(File.ReadAllText(tokenFile), null, System.Globalization.DateTimeStyles.RoundtripKind);
            var ok = DateTime.UtcNow - ts <= ttl;
            try { File.Delete(tokenFile); } catch { }
            return ok;
        }

        static Process StartPayload(string exePath)
        {
            var psi = new ProcessStartInfo(exePath) { UseShellExecute = false };
            return Process.Start(psi);
        }
    }
}


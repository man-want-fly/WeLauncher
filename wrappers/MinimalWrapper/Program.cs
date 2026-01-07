using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

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
            var spec = ReadLaunchSpec(baseDir, appId, token);
            var entryExe = ReadEntryExeFromMeta(appDir);
            var payloadExe = Path.Combine(appDir, "app", entryExe);
            if (!File.Exists(payloadExe)) return 3;
            var p = StartPayload(payloadExe, spec.Args, spec.Env);
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

        static Process StartPayload(string exePath, List<string> args, Dictionary<string,string> env)
        {
            var psi = new ProcessStartInfo(exePath) { UseShellExecute = false };
            foreach (var a in args) psi.ArgumentList.Add(a);
            foreach (var kv in env) psi.Environment[kv.Key] = kv.Value;
            return Process.Start(psi);
        }

        static LaunchSpec ReadLaunchSpec(string baseDir, string appId, string guid)
        {
            var specFile = Path.Combine(baseDir, "launch", $"{appId}-{guid}.json");
            if (File.Exists(specFile))
            {
                var json = File.ReadAllText(specFile);
                try
                {
                    var spec = JsonSerializer.Deserialize<LaunchSpec>(json);
                    try { File.Delete(specFile); } catch { }
                    return spec ?? new LaunchSpec();
                }
                catch
                {
                    return new LaunchSpec();
                }
            }
            return new LaunchSpec();
        }

        static string ReadEntryExeFromMeta(string appDir)
        {
            var metaFile = Path.Combine(appDir, "meta", "app.json");
            if (File.Exists(metaFile))
            {
                var json = File.ReadAllText(metaFile);
                try
                {
                    var meta = JsonSerializer.Deserialize<MetaApp>(json);
                    if (meta != null && !string.IsNullOrWhiteSpace(meta.EntryExe)) return meta.EntryExe;
                }
                catch
                {
                }
            }
            return "App.exe";
        }
    }

    public class LaunchSpec
    {
        public List<string> Args { get; set; } = new List<string>();
        public Dictionary<string, string> Env { get; set; } = new Dictionary<string, string>();
    }

    public class MetaApp
    {
        public string EntryExe { get; set; } = "App.exe";
    }
}

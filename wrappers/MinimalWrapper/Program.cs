using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace MinimalWrapper
{
    class Program
    {
        static int Main(string[] args)
        {
            // 1. Check if Shell is running
            if (!IsShellRunning("WeLauncher")) 
            {
                // Optionally show a message box here if it was a Windows GUI app
                return 1;
            }

            string appDir = "";
            foreach (var a in args)
            {
                if (a.StartsWith("--appDir=")) appDir = a.Substring(9);
            }

            // Fallback: assume we are in [AppDir]/wrapper/WrapperApp.exe
            if (string.IsNullOrEmpty(appDir))
            {
                // Get directory of current executable
                var selfDir = AppDomain.CurrentDomain.BaseDirectory;
                // appDir is parent of selfDir
                appDir = Directory.GetParent(selfDir)?.FullName ?? selfDir;
            }

            var entryExe = ResolveEntryExe(appDir);
            if (string.IsNullOrEmpty(entryExe) || !File.Exists(entryExe)) 
            {
                return 3;
            }

            var p = StartPayload(entryExe);
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

        static Process? StartPayload(string exePath)
        {
            var psi = new ProcessStartInfo(exePath) { UseShellExecute = false };
            // Pass through original arguments if needed? 
            // The user removed LaunchArgs from manifest, so we assume no args needed for now.
            // Or we could pass through args passed to the wrapper (excluding our own flags).
            psi.WorkingDirectory = Path.GetDirectoryName(exePath);
            return Process.Start(psi);
        }

        static string ResolveEntryExe(string appDir)
        {
            // 1. Try meta/app.json
            var metaFile = Path.Combine(appDir, "meta", "app.json");
            if (File.Exists(metaFile))
            {
                try
                {
                    var json = File.ReadAllText(metaFile);
                    var meta = JsonSerializer.Deserialize<MetaApp>(json);
                    if (meta != null && !string.IsNullOrWhiteSpace(meta.EntryExe)) 
                    {
                        return Path.Combine(appDir, "app", meta.EntryExe);
                    }
                }
                catch { }
            }

            // 2. Scan app/ directory for .exe
            var appPayloadDir = Path.Combine(appDir, "app");
            if (Directory.Exists(appPayloadDir))
            {
                var exes = Directory.GetFiles(appPayloadDir, "*.exe");
                if (exes.Length > 0) return exes[0];
            }

            return "";
        }
    }

    public class MetaApp
    {
        public string EntryExe { get; set; } = "";
    }
}

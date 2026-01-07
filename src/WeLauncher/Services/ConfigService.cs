using System;

namespace WeLauncher.Services
{
    public class ConfigService
    {
        public string? GetManifestUrlFromEnv()
        {
            return Environment.GetEnvironmentVariable("LAUNCHER_MANIFEST_URL");
        }
    }
}


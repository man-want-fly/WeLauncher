using System.IO;
using System.Threading.Tasks;
using WeLauncher.Models;

namespace WeLauncher.Services
{
    public class InstallService
    {
        readonly DownloadService _download = new();
        readonly ZipService _zip = new();

        public string GetBaseDir()
        {
            return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "WeLauncher");
        }

        public string GetAppVersionDir(AppDescriptor app)
        {
            return Path.Combine(GetBaseDir(), "apps", app.Id, "versions", app.Version);
        }

        public string GetCacheZipPath(AppDescriptor app)
        {
            return Path.Combine(GetBaseDir(), "cache", "downloads", $"{app.Id}-{app.Version}.zip");
        }

        public async Task InstallAsync(AppDescriptor app)
        {
            Directory.CreateDirectory(Path.Combine(GetBaseDir(), "cache", "downloads"));
            var zipPath = GetCacheZipPath(app);
            await _download.DownloadZipAsync(app.DownloadUrl, zipPath, app.Sha256);
            var destDir = GetAppVersionDir(app);
            _zip.ExtractZip(zipPath, destDir);
        }
    }
}


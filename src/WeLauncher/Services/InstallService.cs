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
            return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "WeLauncher");
        }

        public string GetAppVersionDir(AppDescriptor app)
        {
            return Path.Combine(GetBaseDir(), "apps", app.Id, "versions", app.Version);
        }

        public string GetCacheZipPath(AppDescriptor app)
        {
            return Path.Combine(GetBaseDir(), "cache", "downloads", $"{app.Id}-{app.Version}.zip");
        }

        public bool IsInstalled(AppDescriptor app)
        {
            var destDir = GetAppVersionDir(app);
            // Simple check: directory exists and wrapper exists
            return Directory.Exists(destDir) && File.Exists(Path.Combine(destDir, app.WrapperRelativePath));
        }

        public async Task DownloadAppAsync(AppDescriptor app, System.IProgress<double>? progress = null)
        {
            Directory.CreateDirectory(Path.Combine(GetBaseDir(), "cache", "downloads"));
            var zipPath = GetCacheZipPath(app);
            await _download.DownloadZipAsync(app.DownloadUrl, zipPath, app.Sha256, progress);
        }

        public void ExtractApp(AppDescriptor app)
        {
            var zipPath = GetCacheZipPath(app);
            var destDir = GetAppVersionDir(app);
            _zip.ExtractZip(zipPath, destDir);
        }

        public async Task InstallAsync(AppDescriptor app, System.IProgress<double>? progress = null)
        {
            await DownloadAppAsync(app, progress);
            ExtractApp(app);
        }
    }
}


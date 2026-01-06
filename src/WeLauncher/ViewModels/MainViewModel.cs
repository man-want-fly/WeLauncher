using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using WeLauncher.Models;
using WeLauncher.Services;
using WeLauncher.Utils;

namespace WeLauncher.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<AppDescriptor> Apps { get; } = new();
        public ICommand InstallCommand { get; }
        public ICommand LaunchCommand { get; }

        readonly DownloadService _download = new();
        readonly ZipService _zip = new();
        readonly LaunchService _launch = new();

        public MainViewModel()
        {
            Apps.Add(new AppDescriptor
            {
                Id = "appA",
                Name = "示例应用A",
                Version = "1.0.0",
                Visible = true,
                DownloadUrl = "https://example.com/apps/appA-1.0.0.zip",
                Sha256 = "0000",
                WrapperRelativePath = "wrapper/WrapperApp.exe"
            });

            InstallCommand = new RelayCommand(async o => await InstallAsync((AppDescriptor)o));
            LaunchCommand = new RelayCommand(async o => await LaunchAsync((AppDescriptor)o));
        }

        async Task InstallAsync(AppDescriptor app)
        {
            var baseDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "WeLauncher");
            var appDir = Path.Combine(baseDir, "apps", app.Id, "versions", app.Version);
            Directory.CreateDirectory(Path.Combine(baseDir, "cache", "downloads"));
            Directory.CreateDirectory(appDir);
            var zipPath = Path.Combine(baseDir, "cache", "downloads", $"{app.Id}-{app.Version}.zip");
            try
            {
                await _download.DownloadZipAsync(app.DownloadUrl, zipPath, app.Sha256);
            }
            catch
            {
            }
            try
            {
                _zip.ExtractZip(zipPath, appDir);
            }
            catch
            {
            }
        }

        async Task LaunchAsync(AppDescriptor app)
        {
            var baseDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "WeLauncher");
            var appDir = Path.Combine(baseDir, "apps", app.Id, "versions", app.Version);
            var guid = _launch.CreateLaunchToken(baseDir, app.Id);
            var wrapperExe = Path.Combine(appDir, app.WrapperRelativePath);
            try
            {
                var p = _launch.StartWrapper(wrapperExe, app.Id, guid, appDir);
                await Task.Run(() => p?.WaitForExit());
            }
            catch
            {
            }
        }
    }
}


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

        readonly InstallService _install = new();
        readonly LaunchService _launch = new();
        readonly ManifestService _manifest = new();
        readonly ConfigService _config = new();

        public MainViewModel()
        {
            _ = LoadManifestAsync();

            InstallCommand = new RelayCommand(async o => await InstallAsync((AppDescriptor)o));
            LaunchCommand = new RelayCommand(async o => await LaunchAsync((AppDescriptor)o));
        }

        async Task LoadManifestAsync()
        {
            Manifest? m = null;
            var url = _config.GetManifestUrlFromEnv();
            if (!string.IsNullOrWhiteSpace(url))
            {
                try { m = await _manifest.LoadFromUrlAsync(url); } catch { }
            }
            if (m == null)
            {
                var localPath = Path.Combine(Directory.GetCurrentDirectory(), "manifests", "manifest.sample.json");
                try { m = await _manifest.LoadFromFileAsync(localPath); } catch { }
            }
            if (m == null) return;
            foreach (var a in m.Apps)
            {
                if (a.Visible) Apps.Add(a);
            }
        }

        async Task InstallAsync(AppDescriptor app)
        {
            try { await _install.InstallAsync(app); } catch { }
        }

        async Task LaunchAsync(AppDescriptor app)
        {
            var baseDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "WeLauncher");
            var appDir = _install.GetAppVersionDir(app);
            var guid = _launch.CreateLaunchToken(baseDir, app.Id);
            var spec = new LaunchSpec { AppId = app.Id, AppDir = appDir, Args = app.LaunchArgs, Env = app.Env };
            _launch.WriteLaunchSpec(baseDir, spec, guid);
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

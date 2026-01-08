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
                var localPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "manifest.json");
                try { m = await _manifest.LoadFromFileAsync(localPath); } catch { }
            }
            if (m == null) return;
            foreach (var a in m.Apps)
            {
                Apps.Add(a);
            }
        }

        async Task InstallAsync(AppDescriptor app)
        {
            try { await _install.InstallAsync(app); } catch { }
        }

        async Task LaunchAsync(AppDescriptor app)
        {
            // Simplified launch logic
            var appDir = _install.GetAppVersionDir(app);
            var wrapperExe = Path.Combine(appDir, app.WrapperRelativePath);
            try
            {
                // Just start the wrapper, passing the app directory
                var p = _launch.StartWrapper(wrapperExe, appDir);
                // We don't necessarily need to wait for exit here, but we can if we want to block the UI?
                // The user said "Simple". Usually launcher doesn't block.
                // But previously it awaited. I'll keep await for now, but wrapper logic is independent.
                // If I await, the shell UI might freeze if run on UI thread? 
                // RelayCommand runs async void, so it's fine.
                await Task.Run(() => p?.WaitForExit());
            }
            catch
            {
            }
        }
    }
}

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
        public ICommand AppClickCommand { get; }

        readonly InstallService _install = new();
        readonly LaunchService _launch = new();
        readonly ManifestService _manifest = new();
        readonly ConfigService _config = new();

        public MainViewModel()
        {
            AppClickCommand = new RelayCommand(async o => 
            {
                if (o is AppDescriptor app)
                {
                    await OnAppClickAsync(app);
                }
            });
            _ = LoadManifestAsync();
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
                // Check initial state
                if (_install.IsInstalled(a))
                {
                    a.State = AppState.Ready;
                }
                else
                {
                    a.State = AppState.Idle;
                }
                Apps.Add(a);
            }
        }

        async Task OnAppClickAsync(AppDescriptor app)
        {
            if (app.State == AppState.Downloading || app.State == AppState.Unzipping) return;

            if (app.State == AppState.Ready)
            {
                await LaunchAsync(app);
            }
            else
            {
                await InstallAndLaunchAsync(app);
            }
        }

        async Task InstallAndLaunchAsync(AppDescriptor app)
        {
            try
            {
                app.State = AppState.Downloading;
                app.Progress = 0;
                
                var progress = new System.Progress<double>(p => app.Progress = p * 100);
                await _install.DownloadAppAsync(app, progress);

                app.State = AppState.Unzipping;
                // Run extraction on background thread to avoid freezing UI
                await Task.Run(() => _install.ExtractApp(app));

                app.State = AppState.Ready;
                
                // Auto launch after install
                await LaunchAsync(app);
            }
            catch
            {
                app.State = AppState.Failed;
            }
        }

        async Task LaunchAsync(AppDescriptor app)
        {
            var appDir = _install.GetAppVersionDir(app);
            var wrapperExe = Path.Combine(appDir, app.WrapperRelativePath);
            try
            {
                var p = _launch.StartWrapper(wrapperExe, appDir);
                await Task.Run(() => p?.WaitForExit());
            }
            catch
            {
            }
        }
    }
}

using System.Reflection;

namespace WeLauncher.Services
{
    public class AppInstallerService
    {
        public string GetShellVersion()
        {
            var v = Assembly.GetEntryAssembly()?.GetName().Version;
            return v == null ? "0.0.0.0" : $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
        }
    }
}

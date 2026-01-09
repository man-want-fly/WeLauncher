using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeLauncher.Models
{
    public enum AppState
    {
        Idle,
        Downloading,
        Unzipping,
        Ready,
        Failed
    }

    public class AppDescriptor : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string WrapperRelativePath { get; set; } = "";

        private AppState _state = AppState.Idle;
        public AppState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

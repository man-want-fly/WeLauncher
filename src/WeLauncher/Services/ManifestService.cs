using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeLauncher.Models;

namespace WeLauncher.Services
{
    public class ManifestService
    {
        public async Task<Manifest?> LoadFromFileAsync(string path)
        {
            if (!File.Exists(path)) return null;
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<Manifest>(json);
        }

        public async Task<Manifest?> LoadFromUrlAsync(string url)
        {
            using var http = new HttpClient();
            var json = await http.GetStringAsync(url);
            return JsonSerializer.Deserialize<Manifest>(json);
        }
    }
}

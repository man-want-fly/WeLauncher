using System.IO;
using System.IO.Compression;

namespace WeLauncher.Services
{
    public class ZipService
    {
        public void ExtractZip(string zipPath, string destDir)
        {
            Directory.CreateDirectory(destDir);
            ZipFile.ExtractToDirectory(zipPath, destDir, true);
        }
    }
}


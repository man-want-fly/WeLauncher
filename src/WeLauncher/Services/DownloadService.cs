using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WeLauncher.Services
{
    public class DownloadService
    {
        public async Task<string> DownloadZipAsync(string url, string destPath, string? expectedSha256, System.IProgress<double>? progress = null)
        {
            using var http = new HttpClient();
            using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();
            
            var totalBytes = resp.Content.Headers.ContentLength ?? -1L;
            var readBytes = 0L;

            using var fs = File.Create(destPath);
            using var sha = SHA256.Create();
            using var stream = await resp.Content.ReadAsStreamAsync();
            var buffer = new byte[81920];
            int read;
            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, read);
                sha.TransformBlock(buffer, 0, read, null, 0);
                
                readBytes += read;
                if (totalBytes > 0)
                {
                    progress?.Report((double)readBytes / totalBytes);
                }
            }
            sha.TransformFinalBlock(System.Array.Empty<byte>(), 0, 0);
            var hash = System.Convert.ToHexString(sha.Hash!).ToLowerInvariant();
            if (!string.IsNullOrEmpty(expectedSha256) && hash != expectedSha256.ToLowerInvariant()) throw new IOException("Hash mismatch");
            return destPath;
        }
    }
}


using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ShellProgressBar;

namespace SUs.KeepLatest.Cli
{
    public class FileDownloader
    {
        private static readonly ProgressBarOptions _options = new()
        {
            ForegroundColor = ConsoleColor.Yellow,
            ForegroundColorDone = ConsoleColor.DarkGreen,
            BackgroundColor = ConsoleColor.DarkGray,
            BackgroundCharacter = '\u2593'
        };

        public async Task GetAsync(string fileName, string url)
        {
            using ProgressBar pbar = new(1024, $"Downloading {fileName} ...", _options);
            var progress = pbar.AsProgress<float>();

            using HttpClient httpClient = new();
            var response = await httpClient.GetAsync($"https://github.com/{url.TrimStart('/')}", HttpCompletionOption.ResponseHeadersRead);
            var contentLength = response.Content.Headers.ContentLength;
            var responseStream = await response.Content.ReadAsStreamAsync();

            using var fileStream = File.Create(BuildAbsoluteFileName(fileName), 4096);

            var buffer = new byte[1024];
            var readBytesLength = await responseStream.ReadAsync(buffer, 0, buffer.Length);
            var readBytesCount = (float)readBytesLength;
            while (readBytesLength > 0)
            {
                await fileStream.WriteAsync(buffer, 0, readBytesLength);

                progress.Report(readBytesCount / contentLength.Value);

                readBytesLength = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                readBytesCount += readBytesLength;
            }

            pbar.Message += " - Done!";
        }

        private static string BuildAbsoluteFileName(string fileNmae)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Path.Combine(dir, fileNmae);
        }
    }
}

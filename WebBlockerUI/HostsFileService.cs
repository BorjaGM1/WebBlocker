namespace WebBlockerUI
{
    public class HostsFileService
    {
        private readonly string hostsFilePath = "C:\\Windows\\System32\\drivers\\etc\\hosts";

        public Task<List<WebsiteEntry>> GetWebsiteEntriesAsync()
        {
            return Task.Run(() =>
            {
                var lines = File.ReadAllLines(hostsFilePath);
                var entries = lines
                    .Where(line => line.Contains("0.0.0.0"))
                    .Select(line => new WebsiteEntry
                    {
                        Url = line.Replace("0.0.0.0 ", "").Trim(),
                        IsBlocked = !line.StartsWith("#")
                    })
                    .ToList();
                return entries;
            });
        }

        public List<WebsiteEntry> GetWebsiteEntries()
        {
            var lines = File.ReadAllLines(hostsFilePath);
            var entries = lines
                .Where(line => line.Contains("0.0.0.0"))
                .Select(line => new WebsiteEntry
                {
                    Url = line.Replace("0.0.0.0 ", "").Trim(),
                    IsBlocked = !line.StartsWith("#")
                })
                .ToList();
            return entries;
        }

        public async Task AddWebsite(WebsiteEntry website)
        {
            var lines = new List<string>(await File.ReadAllLinesAsync(hostsFilePath));
            string newEntry = $"0.0.0.0 {website.Url}";

            // Prevent duplicate entries
            if (!lines.Any(line => line.Contains(website.Url)))
            {
                lines.Add(newEntry);
                await File.WriteAllLinesAsync(hostsFilePath, lines);
            }
        }

        public Task BlockWebsiteAsync(string url)
        {
            return Task.Run(() =>
            {
                var lines = File.ReadAllLines(hostsFilePath).ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains(url))
                    {
                        lines[i] = lines[i].TrimStart('#');
                        break;
                    }
                }
                File.WriteAllLines(hostsFilePath, lines);
                FlushDns();
            });
        }

        public void BlockWebsite(string url)
        {
            var lines = File.ReadAllLines(hostsFilePath).ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(url.Replace("#", "").Trim()))
                {
                    lines[i] = lines[i].TrimStart('#');
                    break;
                }
            }
            File.WriteAllLines(hostsFilePath, lines);
            FlushDns();
        }

        public Task UnblockWebsiteAsync(string url)
        {
            return Task.Run(() =>
            {
                var lines = File.ReadAllLines(hostsFilePath).ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains(url))
                    {
                        lines[i] = "#" + lines[i];
                        break;
                    }
                }
                File.WriteAllLines(hostsFilePath, lines);
                FlushDns();
            });
        }

        public void UnblockWebsite(string url)
        {
            var lines = File.ReadAllLines(hostsFilePath).ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(url))
                {
                    lines[i] = "#" + lines[i];
                    break;
                }
            }
            File.WriteAllLines(hostsFilePath, lines);
            FlushDns();
        }

        public Task BlockAllWebsitesAsync()
        {
            return Task.Run(() =>
            {
                var lines = File.ReadAllLines(hostsFilePath).ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("0.0.0.0") && lines[i].StartsWith("#"))
                    {
                        lines[i] = lines[i].TrimStart('#');
                    }
                }
                File.WriteAllLines(hostsFilePath, lines);
                FlushDns();
            });
        }

        public Task UnblockAllWebsitesAsync()
        {
            return Task.Run(() =>
            {
                var lines = File.ReadAllLines(hostsFilePath).ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("0.0.0.0") && !lines[i].StartsWith("#"))
                    {
                        lines[i] = "#" + lines[i];
                    }
                }
                File.WriteAllLines(hostsFilePath, lines);
                FlushDns();
            });
        }

        private void FlushDns()
        {
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "ipconfig";
                process.StartInfo.Arguments = "/flushdns";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();
                Console.WriteLine(process.StandardOutput.ReadToEnd());
            }
        }
    }

    public class WebsiteEntry
    {
        public string Url { get; set; }
        public bool IsBlocked { get; set; }
    }
}

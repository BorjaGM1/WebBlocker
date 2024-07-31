namespace WebBlockerUI
{
    public class BackgroundWebblockerService : IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundWebblockerService> _logger;
        private readonly HostsFileService _hostsFileService;
        private Timer? _timer;

        public BackgroundWebblockerService(ILogger<BackgroundWebblockerService> logger, HostsFileService hostsFileService)
        {
            _logger = logger;
            _hostsFileService = hostsFileService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Website Blocker Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation("Website Blocker Service is working.");

            // Example logic: Toggle block/unblock on all websites
            var websites = _hostsFileService.GetWebsiteEntries();
            foreach (var website in websites)
            {
                if (website.IsBlocked)
                {
                    _hostsFileService.UnblockWebsite(website.Url);
                }
                else
                {
                    _hostsFileService.BlockWebsite(website.Url);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Website Blocker Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

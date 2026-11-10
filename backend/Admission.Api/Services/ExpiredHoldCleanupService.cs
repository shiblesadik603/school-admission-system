using Admission.Api.Data;
using Admission.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Services;

// Runs in the background and changes timed-out HELD holds to EXPIRED.
// Seat counts don't depend on this job (they check ExpiresAt), it just keeps the table clean.
public class ExpiredHoldCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredHoldCleanupService> _logger;

    public ExpiredHoldCleanupService(IServiceScopeFactory scopeFactory, ILogger<ExpiredHoldCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var now = DateTime.UtcNow;

                var count = await db.Holds
                    .Where(h => h.Status == HoldStatus.HELD && h.ExpiresAt <= now)
                    .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.EXPIRED), stoppingToken);

                if (count > 0)
                    _logger.LogInformation("{Count} hold(s) expired", count);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Could not expire old holds, will try again");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}

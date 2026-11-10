using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Data;

public static class DatabaseSetup
{
    // Applies EF migrations on startup. Retries for a while because in Docker
    // the database container may still be starting.
    public static async Task MigrateAsync(WebApplication app)
    {
        var logger = app.Logger;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
                return;
            }
            catch (Exception ex) when (attempt < 15)
            {
                logger.LogWarning("Database not ready ({Message}). Retrying in 2s... ({Attempt}/15)", ex.Message, attempt);
                await Task.Delay(2000);
            }
        }
    }
}

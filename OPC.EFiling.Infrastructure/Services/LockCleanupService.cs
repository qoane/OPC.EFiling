using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.Application.Services
{
    public class LockCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public LockCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.UtcNow;

                // Fix: Removed HasValue and Value as ExpiresAt is a non-nullable DateTime
                var expiredLocks = await dbContext.InstructionLocks
                    .Where(l => l.ExpiresAt < now)
                    .ToListAsync(stoppingToken);

                if (expiredLocks.Any())
                {
                    dbContext.InstructionLocks.RemoveRange(expiredLocks);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

class SmsLogService(IDbContextFactory<BoltonCupDbContext> _dbContextFactory) : ISmsLogService
{
    public async Task LogAsync(string recipient, string body, bool succeeded, string? error, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        db.SmsLogs.Add(new SmsLog
        {
            Recipient = recipient,
            Body = body,
            Succeeded = succeeded,
            Error = error,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

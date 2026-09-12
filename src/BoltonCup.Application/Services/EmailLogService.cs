using BoltonCup.Core;
using BoltonCup.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

class EmailLogService(IDbContextFactory<BoltonCupDbContext> _dbContextFactory) : IEmailLogService
{
    public async Task LogAsync(string recipient, string subject, string templateName, bool succeeded, string? error, Guid? broadcastId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        db.EmailLogs.Add(new EmailLog
        {
            Recipient = recipient,
            Subject = subject,
            TemplateName = templateName,
            Succeeded = succeeded,
            Error = error,
            BroadcastId = broadcastId,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

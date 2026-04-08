using System.Text.Json;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TournamentRegistrationService(BoltonCupDbContext _dbContext) : ITournamentRegistrationService
{
    public async Task<TournamentRegistration?> GetAsync(int tournamentId, int accountId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TournamentRegistrations
            .AsNoTracking()
            .Include(e => e.Account)
            .Include(e => e.Tournament)
            .FirstOrDefaultAsync(t => t.AccountId == accountId && t.TournamentId == tournamentId,
                cancellationToken: cancellationToken);
    }

    
    public async Task UpsertAsync(UpsertTournamentRegistrationCommand command,
        CancellationToken cancellationToken = default)
    {
        var tournament = await _dbContext.Tournaments.FindAsync([command.TournamentId], cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);
        
        var entry = await _dbContext.TournamentRegistrations
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.AccountId == command.AccountId && e.TournamentId == command.TournamentId,
                cancellationToken);
        
        var addNew = entry is null;
        
        entry ??= new TournamentRegistration();
        entry.TournamentId = command.TournamentId;
        entry.AccountId = command.AccountId;
        entry.CurrentStep = command.CurrentStep;
        entry.Payload = command.Payload;
        entry.IsComplete = command.IsComplete;

        if (addNew)
            await _dbContext.TournamentRegistrations.AddAsync(entry, cancellationToken);
        else
            _dbContext.TournamentRegistrations.Update(entry);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    
    public async Task CompleteRegistrationAsync(int accountId, int tournamentId, string paymentId,
        CancellationToken cancellationToken = default)
    {
        var registration = await _dbContext.TournamentRegistrations
                               .AsNoTracking()
                               .Include(e => e.Account)
                               .FirstOrDefaultAsync(x => x.AccountId == accountId && x.TournamentId == tournamentId, cancellationToken) 
                           ?? throw new EntityNotFoundException(nameof(TournamentRegistration), new { tournamentId, accountId });

        if (await _dbContext.Players.FirstOrDefaultAsync(p =>
                p.TournamentId == registration.TournamentId && p.AccountId == registration.AccountId, cancellationToken: cancellationToken) is not null)
        {
            throw new AccountAlreadyInTournamentException(registration.AccountId, registration.TournamentId);
        }
        
        var player = new Player
        {
            TournamentId = tournamentId,
            AccountId = accountId,
            PaymentId = paymentId
        };
        
        if (registration.TryParsePayload(out var data))
        {
            player.Position = data.UserInfo.Position;
            player.JerseySize = data.UserInfo.JerseySize;
            player.CanPlayEitherPosition = data.UserInfo.CanPlayEitherPosition;
            player.Friends = data.UserInfo.Friends;
            player.AgreedToCodeOfConduct = data.Documents.HasAgreedToCodeOfConductWaiver;
            player.AgreedToConcussionWaiver = data.Documents.HasAgreedToConcussionWaiver;
            player.AgreedToCommunicationConsent = data.Documents.HasAgreedToCommunicationConsent;

            registration.Account.Phone = data.UserInfo.Phone;
            _dbContext.Accounts.Update(registration.Account);
        }
        
        _dbContext.Players.Add(player);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}


public static class TournamentRegistrationExtensions
{
    public static bool TryParsePayload(this TournamentRegistration registration, out UserRegistration data)
    {
        data = null!;
        try
        {
            if (string.IsNullOrEmpty(registration.Payload))
                return false;

            var parsedData = JsonSerializer.Deserialize<UserRegistration>(registration.Payload);
            if (parsedData is null)
                return false;
            
            data = parsedData;
            return true;
        }
        catch
        {
            return false;
        }
    }
}


public class UserRegistration
{
    public UserInfo UserInfo { get; set; }
    public Documents Documents { get; set; }
}

public class UserInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string HighestLevel { get; set; }
    public string Phone { get; set; }
    public string JerseySize { get; set; }
    public string Position { get; set; }
    public bool CanPlayEitherPosition { get; set; }
    public string? Friends { get; set; }
}

public class Documents
{
    public bool HasAgreedToCodeOfConductWaiver { get; set; }
    public bool HasAgreedToConcussionWaiver { get; set; }
    public bool HasAgreedToCommunicationConsent { get; set; }
}
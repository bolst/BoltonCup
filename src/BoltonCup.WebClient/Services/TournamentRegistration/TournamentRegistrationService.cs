using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BoltonCup.Sdk;

namespace BoltonCup.WebClient.Services;

public interface ITournamentRegistrationService
{
    Task<TournamentRegistrationContext> GetAsync(int tournamentId);
    Task<TournamentRegistrationContext> SaveAndContinueAsync(TournamentRegistrationContext context);
}


public class TournamentRegistrationService(
    IBoltonCupApi _api
) : ITournamentRegistrationService
{

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
    };

    public async Task<TournamentRegistrationContext> GetAsync(int tournamentId)
    {
        var me = await _api.GetMeAsync()
            ?? throw new InvalidOperationException("Unauthorized access.");
        
        try
        {
            var dto = await _api.GetMyTournamentRegistrationAsync(tournamentId);
            return TryParseDto(dto, out var model) 
                ? new TournamentRegistrationContext(tournamentId, dto.CurrentStep, dto.IsComplete, model) 
                : new TournamentRegistrationContext(tournamentId, 0,  false, GetDefaultRegistrationModel(me));
        }
        catch (ApiException e)
            when (e.StatusCode is 204)
        {
            return new TournamentRegistrationContext(tournamentId, 0, false, GetDefaultRegistrationModel(me));
        }
    }

    public async Task<TournamentRegistrationContext> SaveAndContinueAsync(TournamentRegistrationContext context)
    {
        var newContext = context with { CurrentStep = context.CurrentStep + 1 };
        await _api.UpdateMyTournamentRegistrationAsync(context.TournamentId, new TournamentRegistrationDto
        {
            CurrentStep = newContext.CurrentStep,
            Payload = Serialize(context.Model),
        });
        return newContext;
    }


    private static string Serialize(TournamentRegistrationModel model)
    {
        return JsonSerializer.Serialize(model, _serializerOptions);
    }

    private static bool TryParseDto(TournamentRegistrationDto dto, [NotNullWhen(true)] out TournamentRegistrationModel? model)
    {
        try
        {
            model = JsonSerializer.Deserialize<TournamentRegistrationModel>(dto.Payload, _serializerOptions);
            return model is not null;
        }
        catch
        {
            model = null;
            return false;
        }
    }

    private static TournamentRegistrationModel GetDefaultRegistrationModel(AccountDto me)
    {
        return new TournamentRegistrationModel
        {
            UserInfo = new UserInfoModel
            {
                FirstName = me.FirstName,
                LastName = me.LastName,
                Email = me.Email,
                Phone = me.Phone,
                HighestLevel = me.HighestLevel,
            },
            Documents = new DocumentModel()
        };
    }
}

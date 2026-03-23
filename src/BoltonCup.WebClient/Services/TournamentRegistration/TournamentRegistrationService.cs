using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BoltonCup.Sdk;

namespace BoltonCup.WebClient.Services;

public interface ITournamentRegistrationService
{
    Task<TournamentRegistrationContext> GetAsync(int tournamentId);
    Task<int> SaveAndContinueAsync(int tournamentId, TournamentRegistrationContext context);
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
                ? new TournamentRegistrationContext(dto.CurrentStep, model) 
                : new TournamentRegistrationContext(0, GetDefaultRegistrationModel(me));
        }
        catch (ApiException e)
            when (e.StatusCode is 204)
        {
            return new TournamentRegistrationContext(0, GetDefaultRegistrationModel(me));
        }
    }

    public async Task<int> SaveAndContinueAsync(int tournamentId, TournamentRegistrationContext context)
    {
        var payload = Serialize(context.Model);
        await _api.UpdateMyTournamentRegistrationAsync(tournamentId, new TournamentRegistrationDto
        {
            CurrentStep = context.CurrentStep + 1,
            Payload = payload,
        });
        return context.CurrentStep + 1;
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
                Birthday = me.Birthday,
                HighestLevel = me.HighestLevel,
            },
            Documents = new DocumentModel()
        };
    }
}

using System.Diagnostics.Contracts;

namespace BoltonCup.Shared.Data;



// yes. I am aware that this is not the correct way to do this.
public class SupabaseService
{
    private readonly Supabase.Client _supabase;
    private readonly EmailContentBuilder _emailContentBuilder;
    private readonly IBCData _bcData;

    public SupabaseService(Supabase.Client supabase, IBCData bcData)
    {
        _supabase = supabase;
        _bcData = bcData;
        _emailContentBuilder = new EmailContentBuilder();
    }

    public async Task SendRegistrationEmail(RegisterFormModel form)
    {
        var tournament = await _bcData.GetCurrentTournamentAsync();
        var content = _emailContentBuilder.BuildRegistrationEmail(form, tournament);
        await SendEmail(form.Email, "Registration - Bolton Cup 2025", content);
    }


    public async Task SendConfigEmail(BCAccount account)
    {
        var content = _emailContentBuilder.BuildConfigEmail(account);
        await SendEmail(account.Email, "Profile Link", content);
    }

    private async Task SendEmail(string to, string subject, string html)
    {
        var options = new Supabase.Functions.Client.InvokeFunctionOptions
        {
            Body = new Dictionary<string, object>
            {
                { "name", "Functions" },
                { "to", to },
                { "subject", subject },
                { "html", html }
            }
        };
        await _supabase.Functions.Invoke("resend-email", options: options);
    }
    
    [Pure]
    public async Task<ProfilePicUploadErrorType?> UpdateProfilePictureAsync(string email, byte[] imageBytes, string? extension = ".png")
    {
        try
        {
            var account = await _bcData.GetAccountByEmailAsync(email);
            if (account is null) return ProfilePicUploadErrorType.AccountNotFound;

            var filename = $"{account.FirstName}-{account.LastName}{extension}";
            var options = new Supabase.Storage.FileOptions
            {
                ContentType = "data:image/*;base64",
                Upsert = true,
            };

            await _supabase.Storage.From("profile-pictures").Upload(imageBytes, filename, options);
            var publicUrl = _supabase.Storage.From("profile-pictures").GetPublicUrl(filename);
            await _bcData.UpdateAccountProfilePictureAsync(account.Email, publicUrl);
        }
        catch (Supabase.Storage.Exceptions.SupabaseStorageException exc)
        {
            return ProfilePicUploadErrorType.FileTooLarge;
        }
        catch (Exception e)
        {
            return ProfilePicUploadErrorType.Unknown;
        }
        
        return null;
    }
    
    
    
    private class EmailContentBuilder
    {
        public string BuildRegistrationEmail(RegisterFormModel form, BCTournament? tournament)
        {
            var paymentBody = string.Empty;
            
            if (tournament is not null && tournament.payment_open)
            {
                var paymentLink = form.IsGoalie ? tournament.goalie_payment_link : tournament.player_payment_link;
                paymentBody =
                    $"""
                     <p>To complete your registration, you must pay the $150 fee at the link below. Spots are limited. Make sure to use the same email ({form.Email})!</p>
                                         <p><a href="{paymentLink}">{paymentLink}</a></p>
                     """;
            }
            
            var content = $"""
                           <p><img src="https://iiedqecnfyojvubvugmy.supabase.co/storage/v1/object/public/logos//bc-new.png" style="height: 200px; width: 200px;"></p>
                                               <h3 id="registration-complete">Registration Complete</h3>
                                               <p>Hi {form.FirstName},</p>
                                               <p>You have signed up for BOLTON CUP 2025.</p>
                                               <ul>
                                               <li>Name: {form.FirstName} {form.LastName}</li>
                                               <li>Position: {form.Position}</li>
                                               <li>Highest Level: {form.HighestLevel}</li>
                                               </ul>
                                               {paymentBody}
                                               <h3 id="have-questions-">Have Questions?</h3>
                                               <ul>
                                               <li>Nic Bolton: nicbolton17@icloud.com</li>
                                               </ul>
                           """;
            return content;

        }


        public string BuildConfigEmail(BCAccount account)
        {
            var content = $"""
                           <p><img src="https://iiedqecnfyojvubvugmy.supabase.co/storage/v1/object/public/logos//bc-new.png" style="height: 200px; width: 200px;"></p>
                                               <h3 id="profile-setup-header">Set up your profile</h3>
                                               <p>Hi {account.FirstName},</p>
                                               <p>Below is a link where you can edit your profile. You can set your profile picture, availability, etc.</p>
                                               <ul>
                                               <li>Link: <a href="https://boltoncup.ca/config?pck={account.PCKey}">https://boltoncup.ca/config?pck={account.PCKey}</a></li>
                                               </ul>
                                               <h3 id="have-questions-">Have Questions?</h3>
                                               <ul>
                                               <li>Nic Bolton: nicbolton17@icloud.com</li>
                                               </ul>
                           """;
            return content;
        }

    }
}
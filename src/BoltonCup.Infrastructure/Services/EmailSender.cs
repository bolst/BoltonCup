using System.Text.RegularExpressions;
using BoltonCup.Infrastructure.EmailTemplates;
using BoltonCup.Infrastructure.Settings;
using BoltonCup.Infrastructure.Identity;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RazorLight;

namespace BoltonCup.Infrastructure.Services;

public class EmailSender(
    IOptions<SmtpSettings> smtpSettings, 
    IRazorLightEngine _razorEngine, 
    ILogger<EmailSender> _logger
) : IEmailSender<BoltonCupUser>
{
    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
    
    public Task SendConfirmationLinkAsync(BoltonCupUser user, string email, string confirmationLink)
    {
        Console.WriteLine(_smtpSettings.Password);
        _logger.LogInformation("Sending confirmation link to {UserName} with email {email}", user.UserName, email);
        
        const string subject = "Confirm your email for Bolton Cup";
        var model = new ConfirmationEmailViewModel
        {
            FirstName = user.UserName ?? "Playa",
            ConfirmationLink = confirmationLink
        };
        
        FireAndForgetEmail(email, subject, "Confirmation.ConfirmationEmail", model);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(BoltonCupUser user, string email, string resetLink)
    {
        _logger.LogInformation("Sending password reset link to {UserName} with email {email}", user.UserName, email);
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(BoltonCupUser user, string email, string resetCode)
    {
        _logger.LogInformation("Sending password reset code to {UserName} with email {email}", user.UserName, email);
        throw new NotImplementedException();
    }
    
    private void FireAndForgetEmail(string email, string subject, string templateName, object model)
    {
        Task.Run(async () => 
        {
            try 
            {
                _logger.LogInformation("Compiling template and sending email to {Email}...", email);
                
                var htmlMessage = await _razorEngine.CompileRenderAsync(templateName, model);
                var plainTextMessage = Regex.Replace(htmlMessage, "<.*?>", string.Empty).Trim();
                
                // Build the MimeMessage
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder 
                { 
                    HtmlBody = htmlMessage,
                    TextBody = plainTextMessage,
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Connect and send
                using var client = new SmtpClient();
                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port);
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}.", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
            }
        });
    }
}
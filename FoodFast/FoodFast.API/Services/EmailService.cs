using FoodFast.API.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FoodFast.API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(EmailRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (!request.ToEmails.Any())
        {
            throw new ArgumentException("At least one recipient is required", nameof(request.ToEmails));
        }

        var email = CreateEmail(request);

        using var smtpClient = new SmtpClient();

        try
        {
            await smtpClient.ConnectAsync(_settings.Server,
                                          _settings.Port,
                                          SecureSocketOptions.StartTls);

            await smtpClient.AuthenticateAsync(_settings.UserName,
                                               _settings.Password);

            await smtpClient.SendAsync(email);
        }
        finally
        {
            await smtpClient.DisconnectAsync(quit: true);
        }
    }

    private MimeMessage CreateEmail(EmailRequest request)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(MailboxAddress.Parse(_settings.FromEmail));

        emailMessage.To.AddRange(request.ToEmails.Select(MailboxAddress.Parse));

        emailMessage.Subject = request.Subject;

        var bodyBuilder = new BodyBuilder
        {
            TextBody = request.Message
        };

        foreach (var attachment in request.Attachments)
        {
            bodyBuilder.Attachments.Add(attachment.Name,
                                        attachment.Content,
                                        ContentType.Parse(attachment.ContentType));
        }

        emailMessage.Body = bodyBuilder.ToMessageBody();

        return emailMessage;
    }

}
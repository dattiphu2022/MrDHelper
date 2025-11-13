using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using NKE.BlazorUI.AppData.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MrDHelper.Models;
using MrDHelper.AppData.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettingsOptions _settings;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public EmailService(IOptions<EmailSettingsOptions> settings, UserManager<IdentityUser<Guid>> userManager)
    {
        _settings = settings.Value;
        _userManager = userManager;
    }

    #region Helpers

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    private Result ValidateEmailContent(string email, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return Result.Failure("INVALID_EMAIL", "Điền vào email người nhận hợp lệ.");
        if (string.IsNullOrWhiteSpace(subject) || subject.Length < 10)
            return Result.Failure("INVALID_SUBJECT", "Điền vào chủ đề thư, tối thiểu 10 chữ cái.");
        if (string.IsNullOrWhiteSpace(body) || body.Length < 30)
            return Result.Failure("INVALID_BODY", "Điền vào nội dung email. Tối thiểu 30 chữ cái.");
        return Result.Ok();
    }

    private MimeMessage CreateMessage(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var isHtml = body.Contains('<') && body.Contains('>');
        message.Body = new TextPart(isHtml ? "html" : "plain") { Text = body };

        return message;
    }

    private async Task<Result> SendMessageAsync(MimeMessage message)
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, false);
            await client.AuthenticateAsync(_settings.Username, _settings.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Failure("EMAIL_SENDING_FAILS", ex.Message);
        }
    }

    #endregion

    #region Public Methods

    public async Task<Result> SendEmailAsync(string toEmail, string subject, string body, string? purpose = null)
    {
        var validation = ValidateEmailContent(toEmail, subject, body);
        if (!validation.IsSuccess)
            return validation;

        var message = CreateMessage(toEmail, subject, body);
        return await SendMessageAsync(message);
    }

    public async Task<Result> SendEmailAsync(Guid userId, string subject, string body, string? purpose = null)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || string.IsNullOrWhiteSpace(user.Email))
            return Result.Failure("USER_NOT_FOUND", "Người dùng không tồn tại hoặc không có email.");

        var validation = ValidateEmailContent(user.Email, subject, body);
        if (!validation.IsSuccess)
            return validation;

        var message = CreateMessage(user.Email, subject, body);
        return await SendMessageAsync(message);
    }

    #endregion
}

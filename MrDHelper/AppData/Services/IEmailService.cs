using MrDHelper.Models;

namespace MrDHelper.AppData.Services;

public interface IEmailService
{
    Task<Result> SendEmailAsync(string toEmail, string subject, string body, string? purpose = null);
    Task<Result> SendEmailAsync(Guid userId, string subject, string body, string? purpose = null);
}

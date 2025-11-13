using MrDHelper.Models;

namespace NKE.BlazorUI.AppData.Interface;

public interface IEmailService
{
    Task<Result> SendEmailAsync(string toEmail, string subject, string body, string? purpose = null);
    Task<Result> SendEmailAsync(Guid userId, string subject, string body, string? purpose = null);
}

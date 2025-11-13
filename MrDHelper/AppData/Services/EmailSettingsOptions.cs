// Options models (POCO)
// -------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace MrDHelper.AppData.Services;

public sealed class EmailSettingsOptions
{
    [Required] public string SmtpServer { get; init; }
    [Range(1, 65535)] public int Port { get; init; }
    [Required] public string SenderName { get; init; }
    [Required, EmailAddress] public string SenderEmail { get; init; }
    [Required] public string Username { get; init; }
    [Required] public string AppPassword { get; init; }
}

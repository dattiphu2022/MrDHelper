// Services/EmailServiceWithThrottle.cs
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MrDHelper.Models;
using NKE.BlazorUI.AppData.Interface;

namespace MrDHelper.AppData.Services;

public sealed class EmailServiceWithThrottle : IEmailService
{
    private readonly IEmailService _inner;
    private readonly IMemoryCache _cache;
    private readonly EmailThrottleOptions _opt;

    public EmailServiceWithThrottle(
        IEmailService inner,
        IMemoryCache cache,
        IOptions<EmailThrottleOptions> opt)
    {
        _inner = inner;
        _cache = cache;
        _opt = opt.Value;
    }

    // =====================
    // Helpers
    // =====================
    private static string BuildKey(string toEmail, string subject, string? purpose)
        => $"email-throttle:{purpose ?? "generic"}:{toEmail}:{subject ?? string.Empty}"
            .ToLowerInvariant();

    private static string BuildKey(Guid userId, string subject, string? purpose)
        => $"email-throttle:{purpose ?? "generic"}:userid:{userId}:{subject ?? string.Empty}"
            .ToLowerInvariant();

    private bool ShouldBypass(string? purpose)
        => purpose != null
           && _opt.BypassPurposes.Any(p =>
                string.Equals(p, purpose, StringComparison.OrdinalIgnoreCase));

    private async Task<Result> SendWithThrottleAsync(
        string cacheKey,
        Func<Task<Result>> sendAction,
        string? purpose)
    {
        // Bỏ qua throttle theo danh sách cấu hình
        if (ShouldBypass(purpose))
            return await sendAction();

        // Đang cooldown?
        if (_cache.TryGetValue(cacheKey, out _))
            return Result.Failure("CooldownWating", "Bạn đã gửi gần đây, vui lòng thử lại sau.");

        var r = await sendAction();
        if (r.IsSuccess)
        {
            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(_opt.CooldownSeconds));
        }
        return r;
    }

    // =====================
    // Public API
    // =====================
    public async Task<Result> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        string? purpose = null)
    {
        var key = BuildKey(toEmail, subject, purpose);
        return await SendWithThrottleAsync(
            key,
            () => _inner.SendEmailAsync(toEmail, subject, body, purpose),
            purpose);
    }

    public async Task<Result> SendEmailAsync(
        Guid userId,
        string subject,
        string body,
        string? purpose = null)
    {
        var key = BuildKey(userId, subject, purpose);
        return await SendWithThrottleAsync(
            key,
            () => _inner.SendEmailAsync(userId, subject, body, purpose),
            purpose);
    }
}

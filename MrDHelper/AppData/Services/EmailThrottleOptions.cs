namespace MrDHelper.AppData.Services;

// Services/EmailThrottleOptions.cs
public sealed class EmailThrottleOptions
{
    // Thời gian chờ giữa hai lần gửi cùng "nhóm" (purpose + to + subject)
    public int CooldownSeconds { get; set; } = 120; // 2 phút
                                                    // Các purpose được bỏ qua throttle (ví dụ email hệ thống, báo cáo…)
    public string[] BypassPurposes { get; set; } = Array.Empty<string>();
}

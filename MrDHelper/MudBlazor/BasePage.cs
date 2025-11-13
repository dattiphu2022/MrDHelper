using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MrDHelper.Models;
using MudBlazor;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MrDHelper.MudBlazor;

/// <summary>
/// BasePage cho Blazor Server: gom tiện ích thường dùng cho trang/component.
/// </summary>
public abstract class BasePage : MudComponentBase, IDisposable, IAsyncDisposable
{
    // ===== Inject phổ biến
    [Inject] protected ILoggerFactory LoggerFactory { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    // ===== Busy / Token
    private int _busyDepth = 0;
    protected bool Busy => _busyDepth > 0;
    protected readonly CancellationTokenSource Cts = new();

    protected CancellationToken Token => Cts.Token;

    // ===== Debounce/Throttle
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _debouncers = new();
    private readonly ConcurrentDictionary<string, long> _throttleTicks = new();

    // ===== Before unload guard
    private IJSObjectReference? _jsModule;
    protected bool HasUnsavedChanges { get; set; }

    // ===== Public helpers

    /// <summary>Gói hành động async với Busy + catch + toast + log.</summary>
    protected async Task Wrap(Func<Task> action, string? busyMessage = null, bool toastError = true)
    {
        BeginBusy();
        try
        {
            if (!string.IsNullOrWhiteSpace(busyMessage))
                Snackbar.Add(busyMessage, Severity.Info);

            await action();
        }
        catch (OperationCanceledException) { /* bỏ qua khi hủy */ }
        catch (Exception ex)
        {
            if (toastError) Error(ex.Message);
            CreateLogger()?.LogError(ex, "Wrap() error");
        }
        finally
        {
            EndBusy();
            await StateHasChangedSafe();
        }
    }

    /// <summary>Wrap có trả về kiểu T.</summary>
    protected async Task<T?> Wrap<T>(Func<Task<T>> func, string? busyMessage = null, bool toastError = true)
    {
        BeginBusy();
        try
        {
            if (!string.IsNullOrWhiteSpace(busyMessage))
                Snackbar.Add(busyMessage, Severity.Info);

            return await func();
        }
        catch (OperationCanceledException) { return default; }
        catch (Exception ex)
        {
            if (toastError) Error(ex.Message);
            CreateLogger()?.LogError(ex, "Wrap<T>() error");
            return default;
        }
        finally
        {
            EndBusy();
            await StateHasChangedSafe();
        }
    }

    /// <summary>Chạy pipeline Result T→Result, toast lỗi khi Failure.</summary>
    protected async Task<Result<T>> RunResult<T>(Func<CancellationToken, Task<Result<T>>> action)
    {
        try
        {
            var r = await action(Token);
            if (r.IsFailure) Error(r.FirstError.Description);
            return r;
        }
        catch (OperationCanceledException)
        {
            return Result<T>.Failure("CANCELLED", "Đã hủy tác vụ.");
        }
        catch (Exception ex)
        {
            CreateLogger()?.LogError(ex, "RunResult error");
            Error(ex.Message);
            return Result<T>.Failure("EXCEPTION", ex.Message);
        }
    }

    /// <summary>Hiển thị dialog xác nhận (MudBlazor).</summary>
    protected async Task<bool> ConfirmAsync(string title, string message, string yes = "Đồng ý", string no = "Hủy", Color yesColor = Color.Error)
    {
        var parameters = new DialogParameters
    {
        { "ContentText", message },
        { "ButtonText", yes },
        { "Color", yesColor }
    };

        var options = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, Position = DialogPosition.Center };

        var dialog = await DialogService.ShowAsync<ConfirmDialog>(title, parameters, options);
        var result = await dialog.Result;

        return !result!.Canceled;
    }

    /// <summary>Debounce một hành động theo key: nếu gọi liên tiếp, chỉ chạy lần cuối sau delay.</summary>
    protected async Task DebounceAsync(string key, int delayMs, Func<Task> action)
    {
        var cts = _debouncers.AddOrUpdate(key,
            _ => new CancellationTokenSource(),
            (_, old) => { try { old.Cancel(); old.Dispose(); } catch { } return new CancellationTokenSource(); });

        try
        {
            await Task.Delay(delayMs, cts.Token);
            await action();
        }
        catch (OperationCanceledException) { /* bị debounce */ }
        finally
        {
            _debouncers.TryRemove(key, out var removed);
            removed?.Dispose();
        }
    }

    /// <summary>Throttle: bảo đảm tối thiểu intervalMs giữa hai lần chạy cùng key.</summary>
    protected async Task ThrottleAsync(string key, int intervalMs, Func<Task> action)
    {
        var now = Stopwatch.GetTimestamp();
        var last = _throttleTicks.GetOrAdd(key, 0);
        var ticksPerMs = Stopwatch.Frequency / 1000.0;

        if ((now - last) / ticksPerMs < intervalMs)
            return;

        _throttleTicks[key] = now;
        await action();
    }

    /// <summary>Retry với backoff (ví dụ cho call mạng/DB).</summary>
    protected async Task RetryAsync(Func<CancellationToken, Task> action, int attempts = 3, int initialDelayMs = 200)
    {
        var delay = initialDelayMs;
        for (var i = 1; i <= attempts; i++)
        {
            try
            {
                await action(Token);
                return;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                if (i == attempts)
                {
                    CreateLogger()?.LogError(ex, "Retry failed after {Attempts}", attempts);
                    Error("Thao tác thất bại. Vui lòng thử lại.");
                    throw;
                }
                await Task.Delay(delay, Token);
                delay *= 2; // backoff
            }
        }
    }

    /// <summary>Điều hướng an toàn (kèm forceLoad nếu cần).</summary>
    protected void NavigateSafe(string uri, bool forceLoad = false, bool replace = false)
    {
        try { NavigationManager.NavigateTo(uri, forceLoad: forceLoad, replace: replace); }
        catch (Exception ex) { CreateLogger()?.LogError(ex, "NavigateSafe error: {Uri}", uri); }
    }

    /// <summary>Gắn/hủy cảnh báo "rời trang sẽ mất thay đổi".</summary>
    protected async Task SetBeforeUnloadGuardAsync(bool enable)
    {
        try
        {
            _jsModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./_content/guardBeforeUnload.js");
            await _jsModule.InvokeVoidAsync(enable ? "enableGuard" : "disableGuard");
            HasUnsavedChanges = enable;
        }
        catch (JSDisconnectedException) { /* circuit disposed */ }
        catch (Exception ex) { CreateLogger()?.LogWarning(ex, "SetBeforeUnloadGuardAsync failed"); }
    }

    // ===== Snackbar nhanh
    protected void Success(string msg) => Snackbar.Add(msg, Severity.Success);
    protected void Info(string msg) => Snackbar.Add(msg, Severity.Info);
    protected void Warning(string msg) => Snackbar.Add(msg, Severity.Warning);
    protected void Error(string msg) => Snackbar.Add(msg, Severity.Error);

    // ===== Form helpers
    protected static bool Validate(EditContext? ctx)
        => ctx != null && ctx.Validate();

    // ===== Lifecycle helpers
    protected async Task StateHasChangedSafe()
    {
        try { await InvokeAsync(StateHasChanged); }
        catch (ObjectDisposedException) { /* ignore */ }
        catch (InvalidOperationException) { /* ignore when circuit swapping */ }
    }

    // ===== Busy helpers
    protected void BeginBusy() => Interlocked.Increment(ref _busyDepth);
    protected void EndBusy()
    {
        if (Interlocked.Decrement(ref _busyDepth) < 0)
            Interlocked.Exchange(ref _busyDepth, 0);
    }

    // ===== Logging
    protected ILogger? CreateLogger() => LoggerFactory.CreateLogger(GetType().FullName ?? nameof(BasePage));

    // ===== Dispose
    public virtual void Dispose()
    {
        try { Cts.Cancel(); } catch { }
        foreach (var kv in _debouncers)
            kv.Value.Dispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        Dispose();
        if (_jsModule is not null)
        {
            try { await _jsModule.DisposeAsync(); } catch { }
        }
    }
}

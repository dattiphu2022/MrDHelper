using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MrDHelper.Models;
using System.Security.Cryptography;

namespace MrDHelper.AppData.Services;

public sealed class IdentityPasswordGeneratorService
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IOptions<IdentityOptions> _identityOptions;

    private static readonly char[] _lower = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] _upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] _digits = "0123456789".ToCharArray();
    private static readonly char[] _symbols = "!@#$%^&*()-_=+[]{};:,.<>?/\\|`~".ToCharArray();

    public IdentityPasswordGeneratorService(
        UserManager<IdentityUser<Guid>> userManager,
        IOptions<IdentityOptions> identityOptions)
    {
        _userManager = userManager;
        _identityOptions = identityOptions;
    }

    /// <summary>
    /// Sinh mật khẩu đáp ứng policy hiện tại (IdentityOptions + PasswordValidators).
    /// Thành công: Result<string>.Ok(password).
    /// Thất bại: Result<string>.Failure(errors).
    /// </summary>
    public async Task<Result<string>> GenerateAsync(int? overrideMinLength = null, int maxAttempts = 200)
    {
        var pw = _identityOptions.Value.Password;
        var requiredLength = Math.Max(overrideMinLength ?? pw.RequiredLength, 1);

        // Xây pool ký tự theo policy runtime
        var pools = new List<char[]>();
        if (pw.RequireLowercase) pools.Add(_lower);
        if (pw.RequireUppercase) pools.Add(_upper);
        if (pw.RequireDigit) pools.Add(_digits);
        if (pw.RequireNonAlphanumeric) pools.Add(_symbols);
        if (pools.Count == 0)
            pools.AddRange(new[] { _lower, _upper, _digits }); // fallback dễ nhớ nếu không bắt buộc

        var mergedPool = pools.SelectMany(x => x).Distinct().ToArray();

        // Thử tối đa maxAttempts để tìm candidate hợp lệ
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var chars = new List<char>(capacity: requiredLength);

            // Bảo đảm tối thiểu từng nhóm nếu policy yêu cầu
            if (pw.RequireLowercase) chars.Add(Pick(_lower));
            if (pw.RequireUppercase) chars.Add(Pick(_upper));
            if (pw.RequireDigit) chars.Add(Pick(_digits));
            if (pw.RequireNonAlphanumeric) chars.Add(Pick(_symbols));

            // Bổ sung cho đủ độ dài
            while (chars.Count < requiredLength)
                chars.Add(Pick(mergedPool));

            Shuffle(chars);

            // Đảm bảo số ký tự khác nhau (RequiredUniqueChars)
            if (chars.Distinct().Count() < pw.RequiredUniqueChars)
                continue;

            var candidate = new string(chars.ToArray());

            // Cho toàn bộ PasswordValidators “chấm”
            var dummyUser = new IdentityUser<Guid> { UserName = "pw.generator" };
            var identityErrors = new List<IdentityError>();

            foreach (var validator in _userManager.PasswordValidators)
            {
                var res = await validator.ValidateAsync(_userManager, dummyUser, candidate);
                if (!res.Succeeded && res.Errors != null)
                    identityErrors.AddRange(res.Errors);
            }

            if (identityErrors.Count == 0)
                return Result<string>.Ok(candidate); // ✅ đạt tất cả rule
        }

        // Hết lượt thử mà chưa hợp lệ
        return Result<string>.Failure(
            new Error("Password.GenerateFailed", "Không thể sinh mật khẩu đạt policy sau nhiều lần thử.")
        );
    }

    // Helpers
    private static char Pick(char[] source)
        => source[RandomNumberGenerator.GetInt32(source.Length)];

    private static void Shuffle(List<char> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

}

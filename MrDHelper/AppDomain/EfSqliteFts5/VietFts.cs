using System.Globalization;
using System.Text;


namespace MrDHelper.AppDomain.EfSqliteFts5;


public static class VietFts
{
    // Normalize by removing diacritics, mapping the Vietnamese d variants to plain d, lowercasing, and converting to NFC.
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var s = input.Trim().ToLowerInvariant();
        s = s.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        s = sb.ToString().Normalize(NormalizationForm.FormC);
        s = s.Replace('đ', 'd').Replace('Đ', 'D');

        s = string.Join(" ", s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return s;
    }

    // FTS query format: token AND token AND token, with optional token* prefix matching.
    public static string BuildMatchQuery(string userInput, bool prefix = true)
    {
        var nd = Normalize(userInput);
        if (string.IsNullOrWhiteSpace(nd)) return string.Empty;

        nd = Sanitize(nd);
        var tokens = nd.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => EscapeToken(t))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();

        if (tokens.Length == 0) return string.Empty;

        return string.Join(" AND ", tokens.Select(t => prefix ? $"{t}*" : t));
    }

    private static string Sanitize(string s)
    {
        // Replace characters that would break MATCH syntax with spaces.
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || ch == '_' || ch == ' ')
                sb.Append(ch);
            else
                sb.Append(' ');
        }
        return sb.ToString();
    }

    private static string EscapeToken(string token)
    {
        // Keep only letters, digits, and underscores.
        var sb = new StringBuilder(token.Length);
        foreach (var ch in token)
        {
            if (char.IsLetterOrDigit(ch) || ch == '_')
                sb.Append(ch);
        }
        return sb.ToString();
    }
}

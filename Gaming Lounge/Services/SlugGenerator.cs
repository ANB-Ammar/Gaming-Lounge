using System.Text.RegularExpressions;

namespace Gaming_Lounge.Services;

public static partial class SlugGenerator
{
    public static string Generate(string value)
    {
        var slug = NonAlphaNumericRegex().Replace(value.Trim().ToLowerInvariant(), "-");
        return MultiDashRegex().Replace(slug, "-").Trim('-');
    }

    public static string Generate(string value, int maxLength, string fallback)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

        var slug = Generate(value);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = Generate(fallback);
        }

        return slug.Length <= maxLength
            ? slug
            : slug[..maxLength].TrimEnd('-');
    }

    public static string GenerateUnique(
        string value,
        IEnumerable<string> existingSlugs,
        int maxLength,
        string fallback)
    {
        var baseSlug = Generate(value, maxLength, fallback);
        var existing = existingSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(baseSlug))
        {
            return baseSlug;
        }

        for (var suffix = 2; ; suffix++)
        {
            var suffixText = $"-{suffix}";
            var prefixLength = Math.Max(1, maxLength - suffixText.Length);
            var prefix = baseSlug.Length <= prefixLength
                ? baseSlug
                : baseSlug[..prefixLength].TrimEnd('-');
            var candidate = $"{prefix}{suffixText}";

            if (!existing.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultiDashRegex();
}
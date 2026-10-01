using System.Globalization;
using System.Text;

namespace BoltonCup.Core;

/// <summary>Turns free text into a lowercase, ASCII, hyphen-separated URL segment.</summary>
public static class SlugGenerator
{
    const int MaxLength = 80;
    const string Fallback = "post";

    public static string Generate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Fallback;
        }

        var builder = new StringBuilder(text.Length);
        var previousWasSeparator = true;

        foreach (var character in text.Normalize(NormalizationForm.FormD))
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var lower = char.ToLowerInvariant(character);
            if (lower is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(lower);
                previousWasSeparator = false;
                continue;
            }

            if (!previousWasSeparator)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length > MaxLength)
        {
            slug = slug[..MaxLength].TrimEnd('-');
        }

        return slug.Length == 0 ? Fallback : slug;
    }

    public static string WithSuffix(string slug, int suffix) => $"{slug}-{suffix}";
}

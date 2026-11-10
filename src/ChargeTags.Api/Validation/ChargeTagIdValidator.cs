using System.Text.RegularExpressions;

namespace ChargeTags.Api.Validation;

public static class ChargeTagIdValidator
{
    // "dk-" followed by exactly 10 digits and "-clever"
    private static readonly Regex IdPattern = new Regex("^dk-[0-9]{10}-clever$", RegexOptions.Compiled);

    public static bool IsValid(string? tagId)
    {
        if (string.IsNullOrEmpty(tagId))
        {
            return false;
        }

        return IdPattern.IsMatch(tagId);
    }
}

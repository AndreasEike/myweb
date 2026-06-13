using System.Text.RegularExpressions;
using myweb.api.Models;

namespace myweb.api.Services;

public static class AnswerFormats
{
    private static readonly Regex ScorePattern = new(@"^\d{1,2}-\d{1,2}$", RegexOptions.Compiled);
    private static readonly Regex NumberPattern = new(@"^\d{1,3}$", RegexOptions.Compiled);

    /// <summary>
    /// Normalizes a raw answer to its canonical form ("yes"/"no", "2-1", "home"/"away", "7").
    /// Numeric values are reformatted without leading zeros so scoring can compare strings.
    /// Returns null when the value is not valid for the question type.
    /// </summary>
    public static string? Normalize(QuestionType type, string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var value = raw.Trim().ToLowerInvariant();
        return type switch
        {
            QuestionType.YesNo => value is "yes" or "no" ? value : null,
            QuestionType.TeamPick => value is "home" or "away" ? value : null,
            QuestionType.ScoreGuess => NormalizeScore(value),
            QuestionType.Number => NumberPattern.IsMatch(value) ? int.Parse(value).ToString() : null,
            _ => null
        };
    }

    private static string? NormalizeScore(string value)
    {
        if (!ScorePattern.IsMatch(value))
        {
            return null;
        }
        var parts = value.Split('-');
        return $"{int.Parse(parts[0])}-{int.Parse(parts[1])}";
    }
}

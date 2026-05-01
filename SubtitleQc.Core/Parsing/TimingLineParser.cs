using System.Globalization;

namespace SubtitleQc.Core.Parsing;

/// <summary>
/// Shared helpers for parsing the "HH:MM:SS,mmm --> HH:MM:SS,mmm" or "."
/// variant that SRT and WebVTT use. Centralising this keeps the format
/// parsers thin and avoids duplicated time-parsing bugs.
/// </summary>
internal static class TimingLineParser
{
    private const string ArrowToken = "-->";

    public static (TimeSpan Start, TimeSpan End) ParseTimingLine(string line, char fractionalSeparator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(line);
        int arrowIndex = line.IndexOf(ArrowToken, StringComparison.Ordinal);
        if (arrowIndex < 0)
        {
            throw new SubtitleParseException($"Timing line missing '-->': '{line}'.");
        }

        string left = line[..arrowIndex].Trim();
        string right = line[(arrowIndex + ArrowToken.Length)..].Trim();
        TimeSpan start = ParseTimestamp(left, fractionalSeparator);
        TimeSpan end = ParseTimestamp(StripTrailingCueSettings(right), fractionalSeparator);
        return (start, end);
    }

    private static string StripTrailingCueSettings(string rightSide)
    {
        int firstSpace = rightSide.IndexOf(' ');
        return firstSpace < 0 ? rightSide : rightSide[..firstSpace];
    }

    private static TimeSpan ParseTimestamp(string token, char fractionalSeparator)
    {
        string normalized = fractionalSeparator == '.' ? token : token.Replace(fractionalSeparator, '.');
        string[] parts = normalized.Split(':');
        if (parts.Length is < 2 or > 3)
        {
            throw new SubtitleParseException($"Unrecognised timestamp format: '{token}'.");
        }

        return parts.Length == 3
            ? BuildTimeSpan(parts[0], parts[1], parts[2])
            : BuildTimeSpan("0", parts[0], parts[1]);
    }

    private static TimeSpan BuildTimeSpan(string hours, string minutes, string secondsAndMillis)
    {
        int h = int.Parse(hours, CultureInfo.InvariantCulture);
        int m = int.Parse(minutes, CultureInfo.InvariantCulture);
        double s = double.Parse(secondsAndMillis, CultureInfo.InvariantCulture);
        return new TimeSpan(0, h, m, 0) + TimeSpan.FromSeconds(s);
    }
}

using System.Globalization;

namespace SubtitleQc.Core.Parsing;

/// <summary>
/// Parses TTML time expressions into <see cref="TimeSpan"/>. TTML supports two
/// disjoint grammars: clock-time ("HH:MM:SS[.fraction]") and offset-time
/// ("Ns", "Nms", "Nm", "Nh"). They are intentionally kept in this dedicated
/// helper so the existing SRT/WebVTT timing parser (whose grammar is a strict
/// subset of clock-time) stays untouched (OCP).
/// </summary>
/// <remarks>
/// Frame-based ("Nf") and tick-based ("Nt") metrics are deliberately not
/// supported: their resolution depends on document-level frameRate/tickRate
/// metadata, which would couple this helper to TTML document context. A
/// dedicated, context-aware parser can be introduced in a future iteration
/// without modifying this code.
/// </remarks>
internal static class TtmlTimeParser
{
    public static TimeSpan Parse(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        string trimmed = token.Trim();
        return trimmed.Contains(':') ? ParseClockTime(trimmed) : ParseOffsetTime(trimmed);
    }

    private static TimeSpan ParseClockTime(string token)
    {
        string[] parts = token.Split(':');
        if (parts.Length != 3)
        {
            throw new SubtitleParseException($"Unsupported TTML clock-time format: '{token}'.");
        }

        int hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
        int minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
        double seconds = ParseSeconds(parts[2], token);
        return new TimeSpan(0, hours, minutes, 0) + TimeSpan.FromSeconds(seconds);
    }

    private static double ParseSeconds(string raw, string originalToken)
    {
        string normalized = raw.Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
        {
            throw new SubtitleParseException($"Invalid seconds component in TTML time: '{originalToken}'.");
        }

        return seconds;
    }

    private static TimeSpan ParseOffsetTime(string token)
    {
        (string number, string unit) = SplitOffsetToken(token);
        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new SubtitleParseException($"Invalid TTML offset-time number: '{token}'.");
        }

        return BuildOffset(value, unit, token);
    }

    private static (string Number, string Unit) SplitOffsetToken(string token)
    {
        int splitAt = 0;
        while (splitAt < token.Length && (char.IsDigit(token[splitAt]) || token[splitAt] is '.' or ','))
        {
            splitAt++;
        }

        string number = token[..splitAt].Replace(',', '.');
        string unit = token[splitAt..].ToLowerInvariant();
        return (number, unit);
    }

    private static TimeSpan BuildOffset(double value, string unit, string originalToken)
    {
        return unit switch
        {
            "h" => TimeSpan.FromHours(value),
            "m" => TimeSpan.FromMinutes(value),
            "s" => TimeSpan.FromSeconds(value),
            "ms" => TimeSpan.FromMilliseconds(value),
            _ => throw new SubtitleParseException($"Unsupported TTML time metric in '{originalToken}'. Supported: h, m, s, ms.")
        };
    }
}

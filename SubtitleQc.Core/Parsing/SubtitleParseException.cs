namespace SubtitleQc.Core.Parsing;

/// <summary>
/// Raised when a parser encounters input it cannot map onto the internal
/// data model. Kept as a dedicated exception type so callers can distinguish
/// parse failures from QC failures without inspecting messages.
/// </summary>
public sealed class SubtitleParseException : Exception
{
    public SubtitleParseException(string message)
        : base(message)
    {
    }

    public SubtitleParseException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

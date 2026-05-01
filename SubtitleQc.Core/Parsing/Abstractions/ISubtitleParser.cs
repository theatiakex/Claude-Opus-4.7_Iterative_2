using System.Collections.Generic;
using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Parsing.Abstractions;

/// <summary>
/// Format-specific parser contract. Each parser is fully decoupled from the
/// QC engine and emits the unified internal model so new formats (e.g. TTML)
/// can be added without touching downstream code (OCP).
/// </summary>
public interface ISubtitleParser
{
    /// <summary>
    /// Stable identifier for the format this parser handles (e.g. "SRT").
    /// </summary>
    string FormatId { get; }

    /// <summary>
    /// Parses the supplied raw content into a sequence of internal cues.
    /// Implementations should throw <see cref="SubtitleParseException"/> on
    /// structurally malformed input.
    /// </summary>
    IReadOnlyList<Cue> Parse(string content);
}

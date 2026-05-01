using System.Collections.Generic;
using System.Globalization;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Parsing.Abstractions;

namespace SubtitleQc.Core.Parsing;

/// <summary>
/// SubRip (.srt) parser. SRT blocks are: optional numeric index line, a
/// timing line using ',' as the fractional separator, then one or more text
/// lines. The numeric index is preserved verbatim as the cue id when present
/// so QC reports can reference the original file location.
/// </summary>
public sealed class SrtParser : ISubtitleParser
{
    public string FormatId => "SRT";

    public IReadOnlyList<Cue> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        List<Cue> cues = new();
        int autoId = 1;
        foreach (IReadOnlyList<string> block in SubtitleBlockReader.ReadBlocks(content))
        {
            cues.Add(ParseBlock(block, ref autoId));
        }

        return cues;
    }

    private static Cue ParseBlock(IReadOnlyList<string> block, ref int autoId)
    {
        if (block.Count < 2)
        {
            throw new SubtitleParseException("SRT block must contain a timing line and at least one text line.");
        }

        int timingIndex = TryParseIndex(block[0], out string? explicitId) ? 1 : 0;
        if (timingIndex >= block.Count)
        {
            throw new SubtitleParseException("SRT block missing timing line.");
        }

        (TimeSpan start, TimeSpan end) = TimingLineParser.ParseTimingLine(block[timingIndex], fractionalSeparator: ',');
        IReadOnlyList<string> lines = ExtractTextLines(block, timingIndex + 1);
        string cueId = explicitId ?? autoId.ToString(CultureInfo.InvariantCulture);
        autoId++;
        return new Cue(cueId, start, end, lines);
    }

    private static bool TryParseIndex(string line, out string? id)
    {
        if (int.TryParse(line.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
        {
            id = parsed.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        id = null;
        return false;
    }

    private static IReadOnlyList<string> ExtractTextLines(IReadOnlyList<string> block, int startIndex)
    {
        List<string> textLines = new(block.Count - startIndex);
        for (int i = startIndex; i < block.Count; i++)
        {
            textLines.Add(block[i]);
        }

        return textLines;
    }
}
